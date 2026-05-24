import { getAccessToken, login } from '@/services/auth'
import { appSettings } from '@/services/appConfig'

const apiBaseUrl = appSettings.apiBaseUrl

export type CurrentUserDto = {
  id: string
  email: string
  firstName: string | null
  lastName: string | null
  hasInterview: boolean
}

export type QuestionDto = {
  questionText: string
  answerOptions: string[]
  answerLevels: string[]
  plainTextOptionPresent: boolean
  isSingleChoice: boolean
  isOptional: boolean
}

export type OptionAnswerDto = {
  optionId: number
  selectedLevel: number | null
}

export type AnswerDto = {
  selectedOptions: OptionAnswerDto[]
  textAnswer: string | null
  isSkipped: boolean
}

export type InterviewStepDto = {
  question: QuestionDto | null
  answer: AnswerDto | null
}

export type InterviewSetupDto = {
  hashGuid: string
  requiredQuestions: QuestionDto[]
}

export type SpecializationDto = {
  name: string
  alternativeNames: string[]
}

export type SkillDto = {
  displayName: string
  description: string
  dominanceLevel: string
  alternativeNames: string[]
}

export type ToolDto = {
  toolStandardName: string
  usageFrequency: string
  toolAltNames: string[]
}

export type DomainDto = {
  name: string
  alternativeNames: string[]
}

export type UserProfileDto = {
  cluster: string
  specializations: SpecializationDto[]
  skills: SkillDto[]
  tools: ToolDto[]
  preferredDomains: DomainDto[]
}

export type MyInterviewDisplayDto = {
  setup: InterviewSetupDto | null
  requiredAnswers: AnswerDto[]
  completedSteps: InterviewStepDto[]
  currentQuestion: QuestionDto | null
  conclusion: UserProfileDto | null
}

export type FormElementDto = {
  question: QuestionDto | null
  userProfile: UserProfileDto | null
}

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

async function readErrorMessage(response: Response) {
  try {
    const body = await response.json() as { message?: string }
    return body.message ?? `API request failed with ${response.status}`
  } catch {
    return `API request failed with ${response.status}`
  }
}

async function apiFetch<T>(path: string, init: RequestInit = {}) {
  const token = await getAccessToken()

  if (!token) {
    void login()
    throw new ApiError(401, 'Пользователь не авторизован')
  }

  const headers = new Headers(init.headers)
  headers.set('Authorization', `Bearer ${token}`)
  headers.set('Accept', 'application/json')

  if (init.body) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers,
  })

  if (response.status === 401) {
    void login()
    throw new ApiError(401, 'Сессия истекла')
  }

  if (!response.ok) {
    throw new ApiError(response.status, await readErrorMessage(response))
  }

  return await response.json() as T
}

export function getCurrentUser() {
  return apiFetch<CurrentUserDto>('/me')
}

export function getMyInterview() {
  return apiFetch<MyInterviewDisplayDto>('/my-interview')
}

export function submitInterviewAnswer(answer: AnswerDto) {
  return apiFetch<FormElementDto>('/my-interview/answers', {
    method: 'POST',
    body: JSON.stringify(answer),
  })
}
