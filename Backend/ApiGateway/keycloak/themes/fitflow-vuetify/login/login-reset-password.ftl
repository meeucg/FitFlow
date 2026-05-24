<#import "template.ftl" as layout>
<@layout.registrationLayout bodyClass="fitflow-auth-body--reset" displayInfo=false displayMessage=!messagesPerField.existsError('username'); section>
    <#if section = "header">
        Забыли пароль?
    <#elseif section = "form">
        <form id="kc-reset-password-form" class="fitflow-form" action="${url.loginAction}" method="post">
            <div class="fitflow-field">
                <label for="username" class="fitflow-label">Email</label>
                <input
                    type="email"
                    id="username"
                    name="username"
                    class="fitflow-input"
                    autofocus
                    value="${(auth.attemptedUsername!'')}"
                    autocomplete="email"
                    aria-invalid="<#if messagesPerField.existsError('username')>true</#if>"
                    dir="ltr"
                >
                <#if messagesPerField.existsError('username')>
                    <span id="input-error-username" class="fitflow-field-error" aria-live="polite">
                        ${kcSanitize(messagesPerField.get('username'))?no_esc}
                    </span>
                </#if>
            </div>

            <button class="fitflow-button fitflow-button--primary" type="submit">
                Продолжить
            </button>

            <a class="fitflow-link fitflow-link--center" href="${url.loginUrl}">
                Назад ко входу
            </a>
        </form>
    </#if>
</@layout.registrationLayout>
