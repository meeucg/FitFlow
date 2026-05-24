<template>
  <v-card class="placeholder-hero-card" elevation="0">
    <div class="placeholder-hero-copy">
      <span>Настройки</span>
      <SplitText
        class="placeholder-title"
        tag="h1"
        text="Управление профилем"
        :active="active"
        split-type="words"
        :delay="28"
        :duration="0.78"
        :exit-duration="0.28"
        :blur="4"
      />
      <p>Здесь позже появятся настройки уведомлений, приватности, интеграций и способа подбора заказов.</p>
    </div>

    <v-avatar class="placeholder-hero-icon" size="4rem">
      <v-icon icon="mdi-cog-outline" />
    </v-avatar>
  </v-card>

  <div class="settings-grid">
    <v-card
      v-for="setting in settings"
      :key="setting.title"
      class="settings-card"
      elevation="0"
    >
      <v-avatar class="settings-icon" size="2.8rem">
        <v-icon :icon="setting.icon" />
      </v-avatar>

      <div>
        <h2>{{ setting.title }}</h2>
        <p>{{ setting.description }}</p>

        <div class="placeholder-chip-list">
          <v-chip
            v-for="chip in setting.chips"
            :key="chip"
            class="placeholder-chip"
            variant="flat"
            size="small"
          >
            {{ chip }}
          </v-chip>
        </div>
      </div>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import SplitText from './SplitText.vue'

defineProps<{
  active: boolean
}>()

const settings = [
  {
    title: 'Уведомления',
    description: 'Настройка писем, пушей и важных событий по новым заказам.',
    icon: 'mdi-bell-outline',
    chips: ['Новые заказы', 'Рекомендации', 'Итоги недели'],
  },
  {
    title: 'Подбор заказов',
    description: 'Фильтры по платформам, бюджету, стеку и типам проектов.',
    icon: 'mdi-tune-variant',
    chips: ['Бюджет', 'Площадки', 'Теги'],
  },
  {
    title: 'Приватность',
    description: 'Контроль видимости профиля и данных, которые используются для рекомендаций.',
    icon: 'mdi-shield-outline',
    chips: ['Профиль', 'Данные', 'Безопасность'],
  },
  {
    title: 'Интеграции',
    description: 'Подключение Telegram, почты, календаря и внешних фриланс-площадок.',
    icon: 'mdi-connection',
    chips: ['Telegram', 'Email', 'Площадки'],
  },
]
</script>

<style scoped>
.placeholder-hero-card,
.settings-card {
  border-radius: 2rem;
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 22px 62px rgba(23, 42, 97, 0.14);
  color: black;
}

.placeholder-hero-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1.25rem;
  padding: 1.55rem;
}

.placeholder-hero-copy {
  min-width: 0;
}

.placeholder-hero-card span {
  display: block;
  color: rgba(0, 0, 0, 0.46);
  font-size: 0.78rem;
  font-weight: 600;
  line-height: 1;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.placeholder-title {
  display: block;
  width: 100%;
  margin: 0.35rem 0 0;
  color: black;
  font-size: 2.1rem;
  font-weight: 600;
  line-height: 1.02;
  letter-spacing: -0.04em;
}

.placeholder-title :deep(*) {
  line-height: inherit;
}

.placeholder-hero-card p {
  max-width: 560px;
  margin: 0.65rem 0 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.98rem;
  line-height: 1.45;
}

.placeholder-hero-icon {
  flex: 0 0 auto;
  background: rgba(0, 78, 234, 0.1);
  color: #004eea;
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

.settings-card {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.25rem;
}

.settings-icon {
  flex: 0 0 auto;
  background: rgba(0, 0, 0, 0.045);
  color: rgba(0, 0, 0, 0.62);
}

.settings-card h2 {
  display: block;
  margin: 0;
  color: black;
  font-size: 1.18rem;
  font-weight: 600;
  line-height: 1.15;
  letter-spacing: -0.025em;
}

.settings-card p {
  margin: 0.55rem 0 0;
  color: rgba(0, 0, 0, 0.58);
  font-size: 0.9rem;
  line-height: 1.42;
}

.placeholder-chip-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.42rem;
  margin-top: 0.85rem;
}

.placeholder-chip {
  max-width: 100%;
  min-height: 1.65rem;
  border-radius: 0.65rem;
  padding: 0.15rem 0.38rem;
  background: white;
  color: rgba(0, 0, 0, 0.76);
  font-size: 0.8rem;
  font-weight: 400;
  line-height: 1.2;
}

.placeholder-chip :deep(.v-chip__content) {
  min-width: 0;
  max-width: 100%;
  white-space: normal;
  overflow-wrap: anywhere;
}

@media (max-width: 900px) {
  .settings-grid {
    grid-template-columns: minmax(0, 1fr);
  }

  .placeholder-hero-card {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
