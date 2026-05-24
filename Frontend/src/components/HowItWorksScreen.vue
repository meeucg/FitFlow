<template>
    <div class="how-screen">
        <SplitText
            tag="h2"
            class="screen-title"
            split-type="words"
            :active="active"
            :delay="45"
            text-align="center"
        >
            Как это работает?
        </SplitText>

        <v-row class="how-grid" align="stretch" no-gutters>
            <v-col>
                <RevealOnActive
                    class="card-reveal card-reveal--left"
                    :active="active"
                    :delay="0.1"
                    :y="42"
                >
                    <v-card class="how-card steps-card" elevation="0">
                        <div class="card-eyebrow">
                            <v-icon icon="mdi-auto-fix" />
                            <span>3 шага</span>
                        </div>

                        <h3 class="card-title">
                            От поиска к отклику без ручной рутины
                        </h3>

                        <div class="steps-list">
                            <div
                                v-for="step in steps"
                                :key="step.number"
                                class="step-row"
                            >
                                <v-avatar class="step-number" size="2.35rem">
                                    {{ step.number }}
                                </v-avatar>

                                <div class="step-copy">
                                    <p>{{ step.title }}</p>
                                    <span>{{ step.text }}</span>
                                </div>
                            </div>
                        </div>
                    </v-card>
                </RevealOnActive>
            </v-col>

            <v-col>
                <RevealOnActive
                    class="card-reveal card-reveal--right"
                    :active="active"
                    :delay="0.18"
                    :y="42"
                >
                    <v-card class="how-card preview-card" elevation="0">
                        <div class="source-strip">
                            <v-chip
                                v-for="source in sources"
                                :key="source"
                                class="source-chip"
                                size="small"
                                variant="flat"
                            >
                                {{ source }}
                            </v-chip>
                        </div>

                        <v-card class="ai-note" elevation="0">
                            <v-avatar class="ai-avatar" size="2.4rem">
                                <v-icon icon="mdi-brain" />
                            </v-avatar>

                            <div>
                                <p>ИИ оценил</p>
                                <span>высокий шанс отклика</span>
                            </div>
                        </v-card>

                        <div class="orders-feed">
                            <v-card
                                v-for="order in orders"
                                :key="order.title"
                                class="order-card"
                                elevation="0"
                            >
                                <v-avatar class="order-icon" size="2.35rem">
                                    <v-icon :icon="order.icon" />
                                </v-avatar>

                                <div class="order-copy">
                                    <p>{{ order.title }}</p>
                                    <span>{{ order.meta }}</span>
                                </div>

                                <strong>{{ order.price }}</strong>
                            </v-card>
                        </div>
                    </v-card>
                </RevealOnActive>
            </v-col>
        </v-row>
    </div>
</template>

<script setup lang="ts">
import RevealOnActive from './RevealOnActive.vue';
import SplitText from './SplitText.vue';

withDefaults(defineProps<{
    active?: boolean;
}>(), {
    active: true,
});

const steps = [
    {
        number: 1,
        title: 'Подключаете источники',
        text: 'Добавляете площадки, каналы и чаты, где появляются заказы.',
    },
    {
        number: 2,
        title: 'FitFlow отбирает релевантное',
        text: 'ИИ убирает шум и поднимает задачи, которые подходят дизайнеру.',
    },
    {
        number: 3,
        title: 'Получаете единую ленту',
        text: 'Все подходящие заказы видны в одном месте и готовы к отклику.',
    },
];

const sources = ['Behance', 'Telegram', 'FL.ru'];

const orders = [
    {
        title: 'Дизайн лендинга',
        meta: 'Web design • удалённо',
        price: '60 000 ₽',
        icon: 'mdi-web',
    },
    {
        title: 'Интерфейс приложения',
        meta: 'UI/UX • высокий приоритет',
        price: '$900',
        icon: 'mdi-cellphone',
    },
];
</script>

<style scoped>
.how-screen {
    width: min(72vw, 1120px);
    min-height: 100vh;
    padding-top: calc(var(--header-container-height) + 1.7rem);
    box-sizing: border-box;
    color: white;
}

.screen-title {
    display: block;
    width: 100%;
    color: white;
    font-size: 3.5rem;
    font-weight: 500;
    line-height: 1.05;
    text-align: center;
    letter-spacing: 0;
}

.how-grid {
    margin-top: 2rem;
    display: flex;
    gap: 1.45rem;
}

.how-grid :deep(.v-col) {
    min-width: 0;
    flex: 1 1 0;
}

.card-reveal {
    height: 100%;
}

.how-card {
    height: 270px;
    border-radius: 2rem;
    padding: 1.5rem;
    background: rgba(255, 255, 255, 0.96);
    box-shadow: 0 18px 54px rgba(3, 12, 52, 0.16);
    color: black;
}

.card-eyebrow {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    color: #ff493b;
    font-size: 0.82rem;
    font-weight: 700;
}

.card-eyebrow .v-icon {
    font-size: 1.15rem;
}

.card-title {
    max-width: 360px;
    margin-top: 0.5rem;
    font-size: 1.12rem;
    font-weight: 500;
    line-height: 1.2;
}

.steps-list {
    display: grid;
    gap: 0.58rem;
    margin-top: 0.8rem;
}

.step-row {
    display: flex;
    align-items: flex-start;
    gap: 0.75rem;
}

.step-number {
    flex: 0 0 auto;
    background: #004eea;
    color: white;
    font-size: 0.95rem;
    font-weight: 700;
}

.step-copy {
    min-width: 0;
}

.step-copy p,
.order-copy p,
.ai-note p {
    margin: 0;
    font-size: 0.88rem;
    font-weight: 700;
    line-height: 1.15;
}

.step-copy span,
.order-copy span,
.ai-note span {
    display: block;
    margin-top: 0.2rem;
    color: rgba(0, 0, 0, 0.58);
    font-size: 0.7rem;
    font-weight: 400;
    line-height: 1.2;
}

.preview-card {
    display: flex;
    flex-direction: column;
}

.source-strip {
    display: flex;
    gap: 0.55rem;
}

.source-chip {
    background: rgba(0, 78, 234, 0.1);
    color: #004eea;
    font-weight: 700;
}

.ai-note {
    display: flex;
    align-items: center;
    gap: 0.85rem;
    margin-top: 1rem;
    padding: 0.85rem;
    border-radius: 1.25rem;
    background: #004eea;
    color: white;
}

.ai-note span {
    color: rgba(255, 255, 255, 0.72);
}

.ai-avatar {
    flex: 0 0 auto;
    background: rgba(255, 255, 255, 0.18);
    color: #ff493b;
}

.orders-feed {
    display: grid;
    gap: 0.65rem;
    margin-top: auto;
}

.order-card {
    display: grid;
    grid-template-columns: auto minmax(0, 1fr) auto;
    align-items: center;
    gap: 0.8rem;
    padding: 0.72rem 0.85rem;
    border-radius: 1rem;
    background: rgba(0, 78, 234, 0.07);
}

.order-icon {
    background: white;
    color: #ff493b;
}

.order-copy {
    min-width: 0;
}

.order-card strong {
    color: #004eea;
    font-size: 0.86rem;
    font-weight: 700;
    white-space: nowrap;
}
</style>
