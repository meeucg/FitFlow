<template>
    <div class="pricing-screen">
        <SplitText
            tag="h2"
            class="pricing-title"
            split-type="words"
            :active="active"
            :delay="45"
            text-align="center"
        >
            Тарифы
        </SplitText>

        <SplitText
            tag="p"
            class="pricing-subtitle"
            split-type="words"
            :active="active"
            :delay="32"
            text-align="center"
        >
            Выберите формат, с которого удобно начать
        </SplitText>

        <v-row class="pricing-grid" align="stretch" no-gutters>
            <v-col
                v-for="(plan, index) in plans"
                :key="plan.name"
            >
                <RevealOnActive
                    class="pricing-reveal"
                    :active="active"
                    :delay="0.1 + index * 0.08"
                    :y="42"
                >
                    <v-card
                        class="pricing-card"
                        :class="{ 'pricing-card--featured': plan.featured }"
                        elevation="0"
                    >
                        <div class="plan-header">
                            <v-avatar
                                class="plan-icon"
                                :class="{ 'plan-icon--featured': plan.featured }"
                                size="2.35rem"
                            >
                                <v-icon :icon="plan.icon" />
                            </v-avatar>

                            <v-chip
                                v-if="plan.featured"
                                class="featured-chip"
                                size="small"
                                variant="flat"
                            >
                                Популярный
                            </v-chip>
                        </div>

                        <h3>{{ plan.name }}</h3>

                        <div class="plan-price">
                            <strong>{{ plan.price }}</strong>
                            <span v-if="plan.period">{{ plan.period }}</span>
                        </div>

                        <ul class="plan-features">
                            <li
                                v-for="feature in plan.features"
                                :key="feature"
                            >
                                <v-icon icon="mdi-check" />
                                <span>{{ feature }}</span>
                            </li>
                        </ul>

                        <v-btn
                            class="plan-button"
                            :class="{ 'plan-button--featured': plan.featured }"
                            :color="plan.featured ? '#FF493B' : 'black'"
                            rounded="xl"
                            variant="flat"
                            size="large"
                        >
                            {{ plan.cta }}
                        </v-btn>
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

const plans = [
    {
        name: 'Старт',
        price: '0 ₽',
        period: '',
        icon: 'mdi-seed-outline',
        cta: 'Начать бесплатно',
        features: [
            '10 заказов в день',
            'ручной просмотр ленты',
            'базовые источники',
        ],
        featured: false,
    },
    {
        name: 'Профи',
        price: '990 ₽',
        period: '/мес',
        icon: 'mdi-lightning-bolt',
        cta: 'Попробовать Pro',
        features: [
            'все источники заказов',
            'ИИ-фильтрация и приоритеты',
            'уведомления о новых проектах',
        ],
        featured: true,
    },
    {
        name: 'Студия',
        price: '2490 ₽',
        period: '/мес',
        icon: 'mdi-account-group-outline',
        cta: 'Связаться',
        features: [
            '5 дизайнерских профилей',
            'командная лента заказов',
            'приоритетная аналитика',
        ],
        featured: false,
    },
];
</script>

<style scoped>
.pricing-screen {
    width: min(72vw, 1120px);
    min-height: 100vh;
    padding-top: calc(var(--header-container-height) + 1.7rem);
    box-sizing: border-box;
    color: black;
}

.pricing-title {
    display: block;
    width: 100%;
    color: black;
    font-size: 3.5rem;
    font-weight: 500;
    line-height: 1.05;
    text-align: center;
    letter-spacing: 0;
}

.pricing-subtitle {
    display: block;
    width: 100%;
    margin-top: 0.75rem;
    color: rgba(0, 0, 0, 0.58);
    font-size: 1.1rem;
    font-weight: 400;
    line-height: 1.2;
    text-align: center;
}

.pricing-grid {
    margin-top: 2rem;
    display: flex;
    gap: 1.1rem;
}

.pricing-grid :deep(.v-col) {
    min-width: 0;
    flex: 1 1 0;
}

.pricing-reveal {
    height: 100%;
}

.pricing-card {
    position: relative;
    height: 300px;
    display: flex;
    flex-direction: column;
    border: 1px solid rgba(0, 78, 234, 0.1);
    border-radius: 2rem;
    padding: 1.35rem;
    background: rgba(255, 255, 255, 0.95);
    box-shadow: 0 18px 54px rgba(23, 42, 97, 0.13);
    color: black;
}

.pricing-card--featured {
    border-color: rgba(0, 78, 234, 0.46);
    box-shadow: 0 22px 62px rgba(0, 78, 234, 0.2);
}

.plan-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    min-height: 2.35rem;
}

.plan-icon {
    background: rgba(0, 78, 234, 0.1);
    color: #004eea;
}

.plan-icon--featured {
    background: #004eea;
    color: white;
}

.featured-chip {
    background: rgba(255, 73, 59, 0.12);
    color: #ff493b;
    font-weight: 700;
}

.pricing-card h3 {
    margin-top: 1rem;
    color: black;
    font-size: 1.25rem;
    font-weight: 700;
    line-height: 1;
}

.plan-price {
    display: flex;
    align-items: baseline;
    gap: 0.35rem;
    margin-top: 0.8rem;
}

.plan-price strong {
    color: black;
    font-size: 2.2rem;
    font-weight: 700;
    line-height: 1;
}

.plan-price span {
    color: rgba(0, 0, 0, 0.54);
    font-size: 0.92rem;
    font-weight: 500;
}

.plan-features {
    display: grid;
    gap: 0.55rem;
    margin-top: 1.05rem;
}

.plan-features li {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    color: rgba(0, 0, 0, 0.66);
    font-size: 0.82rem;
    font-weight: 400;
    line-height: 1.2;
}

.plan-features .v-icon {
    flex: 0 0 auto;
    color: #004eea;
    font-size: 1rem;
}

.plan-button {
    width: 100%;
    margin-top: auto;
    color: white;
    font-weight: 700;
}

.plan-button--featured {
    box-shadow: 0 14px 34px rgba(255, 73, 59, 0.28);
}
</style>
