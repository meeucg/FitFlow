<#import "template.ftl" as layout>
<#assign summary = message.summary!''>
<#assign isCookieError = summary?contains("Restart login cookie") || summary?contains("cookie") || summary?contains("Cookie") || summary?contains("устарела")>
<@layout.registrationLayout bodyClass="fitflow-auth-body--expired" displayMessage=false; section>
    <#if section = "header">
        <#if isCookieError>
            Сессия истекла
        <#else>
            Что-то пошло не так
        </#if>
    <#elseif section = "form">
        <#assign backUrl = url.loginRestartFlowUrl!url.loginUrl>
        <#if client?? && client.baseUrl?has_content>
            <#assign backUrl = client.baseUrl>
        </#if>

        <div id="kc-error-message" class="fitflow-page-actions">
            <p class="fitflow-expired-copy">
                <#if isCookieError>
                    Страница входа устарела. Начните вход заново.
                <#else>
                    ${kcSanitize(summary)?no_esc}
                </#if>
            </p>

            <#if !(skipLink??)>
                <a id="backToApplication" class="fitflow-button fitflow-button--primary" href="${backUrl}">
                    Вернуться
                </a>
            </#if>
        </div>
    </#if>
</@layout.registrationLayout>
