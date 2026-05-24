<#import "template.ftl" as layout>
<@layout.registrationLayout displayMessage=!messagesPerField.existsError("otp"); section>
    <#if section = "header">
        Войти
    <#elseif section = "form">
        <form
            id="kc-otp-login-form"
            class="fitflow-form"
            action="${url.loginAction}"
            method="post"
        >
            <#if otpLogin.userOtpCredentials?size gt 1>
                <div class="fitflow-field">
                    <span class="fitflow-label">Устройство</span>
                    <div class="fitflow-otp-devices">
                        <#list otpLogin.userOtpCredentials as otpCredential>
                            <label class="fitflow-radio">
                                <input
                                    type="radio"
                                    name="selectedCredentialId"
                                    value="${otpCredential.id}"
                                    <#if otpCredential.id == otpLogin.selectedCredentialId>checked</#if>
                                >
                                <span>${kcSanitize(otpCredential.userLabel!"Authenticator")?no_esc}</span>
                            </label>
                        </#list>
                    </div>
                </div>
            <#else>
                <input
                    type="hidden"
                    name="selectedCredentialId"
                    value="${otpLogin.selectedCredentialId}"
                >
            </#if>

            <div class="fitflow-field">
                <label for="otp" class="fitflow-label">Одноразовый код</label>
                <input
                    id="otp"
                    name="otp"
                    class="fitflow-input"
                    type="text"
                    inputmode="numeric"
                    autocomplete="one-time-code"
                    autofocus
                    aria-invalid="<#if messagesPerField.existsError('otp')>true</#if>"
                >
                <#if messagesPerField.existsError("otp")>
                    <span id="input-error-otp-code" class="fitflow-field-error" aria-live="polite">
                        ${kcSanitize(messagesPerField.get("otp"))?no_esc}
                    </span>
                </#if>
            </div>

            <button
                class="fitflow-button fitflow-button--primary"
                id="kc-login"
                name="login"
                type="submit"
            >
                Войти
            </button>
        </form>
    </#if>
</@layout.registrationLayout>
