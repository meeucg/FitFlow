<#import "template.ftl" as layout>
<@layout.registrationLayout bodyClass="fitflow-auth-body--totp" displayRequiredFields=false displayMessage=false; section>
    <#if section = "header">
        Защитите аккаунт
    <#elseif section = "form">
        <div class="fitflow-totp-layout">
            <section class="fitflow-totp__form-card">
                <h1 class="fitflow-title fitflow-totp__title">Защитите аккаунт</h1>

                <form action="${url.loginAction}" class="fitflow-form fitflow-form--totp fitflow-totp__form" id="kc-totp-settings-form" method="post">
                    <div class="fitflow-field">
                        <label for="totp" class="fitflow-label">Код из приложения</label>
                        <input
                            type="text"
                            id="totp"
                            name="totp"
                            autocomplete="one-time-code"
                            inputmode="numeric"
                            class="fitflow-input"
                            aria-invalid="<#if messagesPerField.existsError('totp')>true</#if>"
                            dir="ltr"
                            required
                        >

                        <#if messagesPerField.existsError('totp')>
                            <span id="input-error-otp-code" class="fitflow-field-error" aria-live="polite">
                                ${kcSanitize(messagesPerField.get('totp'))?no_esc}
                            </span>
                        </#if>

                        <input type="hidden" id="totpSecret" name="totpSecret" value="${totp.totpSecret}">
                        <#if mode??><input type="hidden" id="mode" name="mode" value="${mode}"></#if>
                    </div>

                    <div class="fitflow-field">
                        <label for="userLabel" class="fitflow-label">Название устройства</label>
                        <input
                            type="text"
                            class="fitflow-input"
                            id="userLabel"
                            name="userLabel"
                            autocomplete="off"
                            placeholder="Например, мой телефон"
                            aria-invalid="<#if messagesPerField.existsError('userLabel')>true</#if>"
                        >

                        <#if messagesPerField.existsError('userLabel')>
                            <span id="input-error-otp-label" class="fitflow-field-error" aria-live="polite">
                                ${kcSanitize(messagesPerField.get('userLabel'))?no_esc}
                            </span>
                        </#if>
                    </div>

                    <div class="fitflow-checkbox">
                        <input type="checkbox" id="logout-sessions" name="logout-sessions" value="on" checked>
                        <label for="logout-sessions">Выйти из других сеансов</label>
                    </div>

                    <#if isAppInitiatedAction??>
                        <div class="fitflow-action-grid">
                            <input type="submit" class="fitflow-button fitflow-button--primary" id="saveTOTPBtn" value="Продолжить">
                            <button type="submit" class="fitflow-button fitflow-button--secondary" id="cancelTOTPBtn" name="cancel-aia" value="true">
                                Отмена
                            </button>
                        </div>
                    <#else>
                        <input type="submit" class="fitflow-button fitflow-button--primary" id="saveTOTPBtn" value="Продолжить">
                    </#if>
                </form>
            </section>

            <aside class="fitflow-totp__panel" aria-label="Настройка приложения-аутентификатора">
                <#if mode?? && mode = "manual">
                    <div class="fitflow-totp__manual">
                        <span class="fitflow-totp__eyebrow">Ключ настройки</span>
                        <strong id="kc-totp-secret-key">${totp.totpSecretEncoded}</strong>
                        <a class="fitflow-link" href="${totp.qrUrl}" id="mode-barcode">Показать QR-код</a>
                    </div>
                <#else>
                    <img
                        id="kc-totp-secret-qr-code"
                        class="fitflow-totp__qr"
                        src="data:image/png;base64, ${totp.totpSecretQrCode}"
                        alt="QR-код для настройки аутентификатора"
                    >
                    <a class="fitflow-link" href="${totp.manualUrl}" id="mode-manual">Не получается отсканировать?</a>
                </#if>

                <div class="fitflow-totp__apps">
                    <span>Google Authenticator</span>
                    <span>Microsoft Authenticator</span>
                    <span>FreeOTP</span>
                </div>
            </aside>
        </div>
    </#if>
</@layout.registrationLayout>
