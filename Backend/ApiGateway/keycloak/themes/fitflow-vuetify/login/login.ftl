<#import "template.ftl" as layout>
<@layout.registrationLayout bodyClass="fitflow-auth-body--login" displayMessage=!messagesPerField.existsError('username','password') displayInfo=realm.password && realm.registrationAllowed && !registrationDisabled??; section>
    <#if section = "header">
        Вход
    <#elseif section = "form">
        <#if realm.password>
            <form id="kc-form-login" class="fitflow-form" onsubmit="login.disabled = true; return true;" action="${url.loginAction}" method="post">
                <#if !usernameHidden??>
                    <div class="fitflow-field">
                        <label for="username" class="fitflow-label">Email</label>
                        <input
                            tabindex="1"
                            id="username"
                            class="fitflow-input"
                            name="username"
                            value="${(login.username!'')}"
                            type="email"
                            autofocus
                            autocomplete="username"
                            aria-invalid="<#if messagesPerField.existsError('username','password')>true</#if>"
                            dir="ltr"
                        >
                    </div>
                </#if>

                <div class="fitflow-field">
                    <label for="password" class="fitflow-label">Пароль</label>
                    <div class="fitflow-password-field" dir="ltr">
                        <input
                            tabindex="2"
                            id="password"
                            class="fitflow-input fitflow-input--password"
                            name="password"
                            type="password"
                            autocomplete="current-password"
                            aria-invalid="<#if messagesPerField.existsError('username','password')>true</#if>"
                        >
                        <button
                            class="fitflow-password-toggle"
                            type="button"
                            aria-label="${msg('showPassword')}"
                            aria-controls="password"
                            data-password-toggle
                            tabindex="3"
                            data-icon-show="fitflow-eye fitflow-eye--show"
                            data-icon-hide="fitflow-eye fitflow-eye--hide"
                            data-label-show="${msg('showPassword')}"
                            data-label-hide="${msg('hidePassword')}"
                        >
                            <i class="fitflow-eye fitflow-eye--show" aria-hidden="true"></i>
                        </button>
                    </div>

                    <#if messagesPerField.existsError('username','password')>
                        <span id="input-error" class="fitflow-field-error" aria-live="polite">
                            ${kcSanitize(messagesPerField.getFirstError('username','password'))?no_esc}
                        </span>
                    </#if>
                </div>

                <#if realm.resetPasswordAllowed>
                    <div class="fitflow-form-row fitflow-form-row--end">
                        <a class="fitflow-link" tabindex="4" href="${url.loginResetCredentialsUrl}">Забыли пароль?</a>
                    </div>
                </#if>

                <button
                    tabindex="5"
                    class="fitflow-button fitflow-button--primary"
                    name="login"
                    id="kc-login"
                    type="submit"
                >
                    Войти
                </button>
            </form>
            <script type="module" src="${url.resourcesPath}/js/passwordVisibility.js"></script>
        </#if>
    <#elseif section = "info">
        <div class="fitflow-register-link">
            <span>Еще нет аккаунта?</span>
            <a class="fitflow-link" tabindex="6" href="${url.registrationUrl}">Зарегистрироваться</a>
        </div>
    </#if>
</@layout.registrationLayout>
