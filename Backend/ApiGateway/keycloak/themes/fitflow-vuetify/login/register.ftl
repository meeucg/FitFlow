<#import "template.ftl" as layout>
<@layout.registrationLayout bodyClass="fitflow-auth-body--register" displayMessage=messagesPerField.exists('global') displayRequiredFields=false; section>
    <#if section = "header">
        Создать аккаунт
    <#elseif section = "form">
        <#assign hasAccountErrors = messagesPerField.existsError('email','username','password','password-confirm')>
        <#assign initialStep = hasAccountErrors?then('account','profile')>
        <form
            id="kc-register-form"
            class="fitflow-form fitflow-register"
            action="${url.registrationAction}"
            method="post"
            data-register-flow
            data-initial-step="${initialStep}"
            novalidate
        >
            <div class="fitflow-register__progress" aria-hidden="true">
                <span class="fitflow-register__dot" data-step-dot="profile"></span>
                <span class="fitflow-register__line"></span>
                <span class="fitflow-register__dot" data-step-dot="account"></span>
            </div>

            <section class="fitflow-register__step" data-register-step="profile">
                <div class="fitflow-field">
                    <label for="firstName" class="fitflow-label">Имя</label>
                    <input
                        id="firstName"
                        class="fitflow-input"
                        name="firstName"
                        value="${(register.formData.firstName!'')}"
                        type="text"
                        autocomplete="given-name"
                        aria-invalid="<#if messagesPerField.existsError('firstName')>true</#if>"
                    >
                    <#if messagesPerField.existsError('firstName')>
                        <span id="input-error-firstName" class="fitflow-field-error" aria-live="polite">
                            ${kcSanitize(messagesPerField.get('firstName'))?no_esc}
                        </span>
                    </#if>
                </div>

                <div class="fitflow-field">
                    <label for="lastName" class="fitflow-label">Фамилия</label>
                    <input
                        id="lastName"
                        class="fitflow-input"
                        name="lastName"
                        value="${(register.formData.lastName!'')}"
                        type="text"
                        autocomplete="family-name"
                        aria-invalid="<#if messagesPerField.existsError('lastName')>true</#if>"
                    >
                    <#if messagesPerField.existsError('lastName')>
                        <span id="input-error-lastName" class="fitflow-field-error" aria-live="polite">
                            ${kcSanitize(messagesPerField.get('lastName'))?no_esc}
                        </span>
                    </#if>
                </div>

                <div class="fitflow-action-grid">
                    <button type="button" class="fitflow-button fitflow-button--ghost" data-register-skip>
                        Пропустить
                    </button>
                    <button type="button" class="fitflow-button fitflow-button--primary" data-register-profile-next>
                        Далее
                    </button>
                </div>
            </section>

            <section class="fitflow-register__step" data-register-step="account" hidden>
                <div class="fitflow-field">
                    <label for="email" class="fitflow-label">Email</label>
                    <input
                        id="email"
                        class="fitflow-input"
                        name="email"
                        value="${(register.formData.email!'')}"
                        type="email"
                        autocomplete="email"
                        aria-invalid="<#if messagesPerField.existsError('email','username')>true</#if>"
                        dir="ltr"
                        required
                    >
                    <span class="fitflow-field-error" data-local-error-for="email" hidden></span>
                    <#if messagesPerField.existsError('email','username')>
                        <span id="input-error-email" class="fitflow-field-error" aria-live="polite">
                            ${kcSanitize(messagesPerField.getFirstError('email','username'))?no_esc}
                        </span>
                    </#if>
                </div>

                <#if passwordRequired??>
                    <div class="fitflow-field">
                        <label for="password" class="fitflow-label">Пароль</label>
                        <div class="fitflow-password-field" dir="ltr">
                            <input
                                type="password"
                                id="password"
                                class="fitflow-input fitflow-input--password"
                                name="password"
                                autocomplete="new-password"
                                aria-invalid="<#if messagesPerField.existsError('password','password-confirm')>true</#if>"
                                required
                            >
                            <button
                                class="fitflow-password-toggle"
                                type="button"
                                aria-label="${msg('showPassword')}"
                                aria-controls="password"
                                data-password-toggle
                                data-icon-show="fitflow-eye fitflow-eye--show"
                                data-icon-hide="fitflow-eye fitflow-eye--hide"
                                data-label-show="${msg('showPassword')}"
                                data-label-hide="${msg('hidePassword')}"
                            >
                                <i class="fitflow-eye fitflow-eye--show" aria-hidden="true"></i>
                            </button>
                        </div>
                        <span class="fitflow-field-error" data-local-error-for="password" hidden></span>
                        <#if messagesPerField.existsError('password')>
                            <span id="input-error-password" class="fitflow-field-error" aria-live="polite">
                                ${kcSanitize(messagesPerField.get('password'))?no_esc}
                            </span>
                        </#if>
                    </div>

                    <div class="fitflow-field">
                        <label for="password-confirm" class="fitflow-label">Повторите пароль</label>
                        <div class="fitflow-password-field" dir="ltr">
                            <input
                                type="password"
                                id="password-confirm"
                                class="fitflow-input fitflow-input--password"
                                name="password-confirm"
                                autocomplete="new-password"
                                aria-invalid="<#if messagesPerField.existsError('password-confirm')>true</#if>"
                                required
                            >
                            <button
                                class="fitflow-password-toggle"
                                type="button"
                                aria-label="${msg('showPassword')}"
                                aria-controls="password-confirm"
                                data-password-toggle
                                data-icon-show="fitflow-eye fitflow-eye--show"
                                data-icon-hide="fitflow-eye fitflow-eye--hide"
                                data-label-show="${msg('showPassword')}"
                                data-label-hide="${msg('hidePassword')}"
                            >
                                <i class="fitflow-eye fitflow-eye--show" aria-hidden="true"></i>
                            </button>
                        </div>
                        <span class="fitflow-field-error" data-local-error-for="password-confirm" hidden></span>
                        <#if messagesPerField.existsError('password-confirm')>
                            <span id="input-error-password-confirm" class="fitflow-field-error" aria-live="polite">
                                ${kcSanitize(messagesPerField.get('password-confirm'))?no_esc}
                            </span>
                        </#if>
                    </div>
                </#if>

                <button type="submit" class="fitflow-button fitflow-button--primary">
                    Создать аккаунт
                </button>

                <button type="button" class="fitflow-link-button fitflow-link--center" data-register-account-back>
                    Назад
                </button>
            </section>

            <div class="fitflow-register-link">
                <a class="fitflow-link" href="${url.loginUrl}">Уже есть аккаунт?</a>
            </div>
        </form>

        <script type="module" src="${url.resourcesPath}/js/passwordVisibility.js"></script>
        <script>
            (() => {
                const form = document.querySelector("[data-register-flow]");
                if (!form) {
                    return;
                }

                const accountStep = form.querySelector('[data-register-step="account"]');
                const profileStep = form.querySelector('[data-register-step="profile"]');
                const profileNextButton = form.querySelector("[data-register-profile-next]");
                const accountBackButton = form.querySelector("[data-register-account-back]");
                const skipButton = form.querySelector("[data-register-skip]");
                const email = form.querySelector("#email");
                const password = form.querySelector("#password");
                const confirmPassword = form.querySelector("#password-confirm");
                const firstName = form.querySelector("#firstName");
                const lastName = form.querySelector("#lastName");

                const setStep = (step) => {
                    form.dataset.currentStep = step;
                    accountStep.hidden = step !== "account";
                    profileStep.hidden = step !== "profile";
                };

                const setError = (input, message) => {
                    const error = form.querySelector('[data-local-error-for="' + input.id + '"]');
                    input.setAttribute("aria-invalid", message ? "true" : "false");
                    if (!error) {
                        return;
                    }

                    error.hidden = !message;
                    error.textContent = message || "";
                };

                const showAccountStep = () => {
                    setStep("account");
                    email?.focus();
                };

                const validateAccountStep = () => {
                    let valid = true;
                    const emailValue = email.value.trim();
                    const passwordValue = password?.value ?? "";
                    const confirmValue = confirmPassword?.value ?? "";

                    if (!emailValue) {
                        setError(email, "Введите email.");
                        valid = false;
                    } else if (!email.checkValidity()) {
                        setError(email, "Введите корректный email.");
                        valid = false;
                    } else {
                        setError(email, "");
                    }

                    if (password && !passwordValue) {
                        setError(password, "Введите пароль.");
                        valid = false;
                    } else if (password) {
                        setError(password, "");
                    }

                    if (confirmPassword && !confirmValue) {
                        setError(confirmPassword, "Повторите пароль.");
                        valid = false;
                    } else if (confirmPassword && passwordValue !== confirmValue) {
                        setError(confirmPassword, "Пароли не совпадают.");
                        valid = false;
                    } else if (confirmPassword) {
                        setError(confirmPassword, "");
                    }

                    return valid;
                };

                profileNextButton?.addEventListener("click", showAccountStep);

                accountBackButton?.addEventListener("click", () => {
                    setStep("profile");
                    firstName?.focus();
                });

                skipButton?.addEventListener("click", () => {
                    if (firstName) {
                        firstName.value = "";
                    }
                    if (lastName) {
                        lastName.value = "";
                    }
                    showAccountStep();
                });

                form.addEventListener("submit", (event) => {
                    if (form.dataset.currentStep !== "account") {
                        event.preventDefault();
                        showAccountStep();
                        return;
                    }

                    if (!validateAccountStep()) {
                        event.preventDefault();
                    }
                });

                setStep(form.dataset.initialStep || "profile");
            })();
        </script>
    </#if>
</@layout.registrationLayout>
