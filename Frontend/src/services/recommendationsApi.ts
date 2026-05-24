import { getAccessToken, login } from '@/services/auth'
import { appSettings } from '@/services/appConfig'
import { ApiError, getCurrentUser } from '@/services/interviewApi'

const apiBaseUrl = appSettings.apiBaseUrl
const recommendationCacheKeyPrefix = 'fitflow.recommendations.v1'
let currentRecommendationUserId: string | null = null

export const emptyRecommendationCursor = '0001-01-01T00:00:00Z'

export type RecommendationCache =
  | null
  | {
      ids: string[]
      latestRecommendationAt: string
    }

export type RecommendationSseCommand =
  | { command_name: 'send_batch'; data: { ids: string[] } }
  | { command_name: 'send_single'; data: { id: string } }

export type RecommendationStreamState = 'connecting' | 'connected' | 'reconnecting' | 'closed' | 'error'

export type PostingAttachmentDto = {
  url: string | null
  base64: string | null
  extension: string
}

export type NamedAliasesDto = {
  name: string
  alternativeNames: string[]
}

export type JobPostingSkillDto = {
  displayName: string
  description: string
  alternativeNames: string[]
}

export type ToolAliasesDto = {
  toolStandardName: string
  toolAltNames: string[]
}

export type JobPostingDto = {
  id: string
  source: string
  postedAt: string
  url: string
  author: string | null
  title: string | null
  priceMin: string | null
  priceMax: string | null
  currency: string
  description: string | null
  cluster: string | null
  attachedFiles: PostingAttachmentDto[]
  specializations: NamedAliasesDto[]
  requiredSkills: JobPostingSkillDto[]
  bonusSkills: JobPostingSkillDto[]
  requiredTools: ToolAliasesDto[]
  bonusTools: ToolAliasesDto[]
  domains: NamedAliasesDto[]
}

function createEmptyRecommendationCache(): Exclude<RecommendationCache, null> {
  return {
    ids: [],
    latestRecommendationAt: emptyRecommendationCursor,
  }
}

function getCurrentRecommendationCacheKey() {
  return currentRecommendationUserId
    ? `${recommendationCacheKeyPrefix}.${currentRecommendationUserId}`
    : null
}

function removeLegacyRecommendationCache() {
  window.localStorage.removeItem(recommendationCacheKeyPrefix)
}

export function setRecommendationCacheUserId(userId: string | null) {
  currentRecommendationUserId = userId
}

async function ensureRecommendationCacheUserId() {
  if (currentRecommendationUserId) {
    return currentRecommendationUserId
  }

  const user = await getCurrentUser()
  setRecommendationCacheUserId(user.id)
  return user.id
}

export function loadRecommendationCache(): RecommendationCache {
  const cacheKey = getCurrentRecommendationCacheKey()
  if (!cacheKey) {
    return createEmptyRecommendationCache()
  }

  removeLegacyRecommendationCache()

  const raw = window.localStorage.getItem(cacheKey)

  if (raw === null) {
    return createEmptyRecommendationCache()
  }

  try {
    const parsed = JSON.parse(raw) as RecommendationCache

    if (parsed === null) {
      return null
    }

    if (!Array.isArray(parsed.ids) || typeof parsed.latestRecommendationAt !== 'string') {
      window.localStorage.removeItem(cacheKey)
      return createEmptyRecommendationCache()
    }

    return {
      ids: parsed.ids.filter((id) => typeof id === 'string'),
      latestRecommendationAt: parsed.latestRecommendationAt,
    }
  } catch {
    window.localStorage.removeItem(cacheKey)
    return createEmptyRecommendationCache()
  }
}

export function saveRecommendationCache(cache: RecommendationCache) {
  const cacheKey = getCurrentRecommendationCacheKey()
  if (!cacheKey) {
    return
  }

  removeLegacyRecommendationCache()
  window.localStorage.setItem(cacheKey, JSON.stringify(cache))
}

export async function clearRecommendationCacheToPreparing() {
  await ensureRecommendationCacheUserId()
  saveRecommendationCache(null)
}

export function openRecommendationStream(
  onCommand: (command: RecommendationSseCommand, latestRecommendationAt: string | null) => void,
  onStateChange: (state: RecommendationStreamState) => void = () => {},
) {
  let closed = false
  let abortController: AbortController | null = null

  async function connectLoop() {
    while (!closed) {
      try {
        onStateChange('connecting')
        abortController = new AbortController()

        const token = await getAccessToken()
        if (!token) {
          void login()
          throw new ApiError(401, 'User is not authenticated')
        }

        await ensureRecommendationCacheUserId()
        const cache = loadRecommendationCache()
        const cursor = cache === null ? 'null' : cache.latestRecommendationAt
        const response = await fetch(
          `${apiBaseUrl}/my-recommendations/events?cursor=${encodeURIComponent(cursor)}`,
          {
            headers: {
              Accept: 'text/event-stream',
              Authorization: `Bearer ${token}`,
            },
            signal: abortController.signal,
          },
        )

        if (response.status === 401) {
          void login()
          throw new ApiError(401, 'Session expired')
        }

        if (!response.ok || !response.body) {
          throw new ApiError(response.status, `Recommendation stream failed with ${response.status}`)
        }

        onStateChange('connected')
        await parseSseStream(response.body, onCommand, abortController.signal)
      } catch (error) {
        if (closed || abortController?.signal.aborted) {
          break
        }

        onStateChange('error')
        await delay(1800)
        onStateChange('reconnecting')
      }
    }

    onStateChange('closed')
  }

  void connectLoop()

  return {
    close() {
      closed = true
      abortController?.abort()
    },
  }
}

export async function getJobPosting(id: string) {
  const token = await getAccessToken()

  if (!token) {
    void login()
    throw new ApiError(401, 'User is not authenticated')
  }

  const response = await fetch(`${apiBaseUrl}/job-postings/${encodeURIComponent(id)}`, {
    headers: {
      Accept: 'application/json',
      Authorization: `Bearer ${token}`,
    },
  })

  if (response.status === 401) {
    void login()
    throw new ApiError(401, 'Session expired')
  }

  if (!response.ok) {
    throw new ApiError(response.status, `Job posting request failed with ${response.status}`)
  }

  return await response.json() as JobPostingDto
}

async function parseSseStream(
  body: ReadableStream<Uint8Array>,
  onCommand: (command: RecommendationSseCommand, latestRecommendationAt: string | null) => void,
  signal: AbortSignal,
) {
  const reader = body.getReader()
  const decoder = new TextDecoder()
  let buffer = ''

  try {
    while (!signal.aborted) {
      const { done, value } = await reader.read()

      if (done) {
        break
      }

      buffer += decoder.decode(value, { stream: true })
      const frames = buffer.split(/\r?\n\r?\n/)
      buffer = frames.pop() ?? ''

      for (const frame of frames) {
        const parsed = parseSseFrame(frame)
        if (!parsed.data) {
          continue
        }

        onCommand(JSON.parse(parsed.data) as RecommendationSseCommand, parsed.id)
      }
    }
  } finally {
    reader.releaseLock()
  }
}

function parseSseFrame(frame: string) {
  let id: string | null = null
  const dataLines: string[] = []

  for (const line of frame.split(/\r?\n/)) {
    if (line.startsWith(':')) {
      continue
    }

    if (line.startsWith('id:')) {
      id = line.slice(3).trim()
      continue
    }

    if (line.startsWith('data:')) {
      dataLines.push(line.slice(5).trimStart())
    }
  }

  return {
    id,
    data: dataLines.join('\n'),
  }
}

function delay(ms: number) {
  return new Promise((resolve) => window.setTimeout(resolve, ms))
}
