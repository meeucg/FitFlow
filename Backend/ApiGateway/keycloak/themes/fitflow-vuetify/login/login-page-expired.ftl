<#import "template.ftl" as layout>
<@layout.registrationLayout bodyClass="fitflow-auth-body--expired" displayMessage=false; section>
    <#if section = "header">
        Сессия истекла
    <#elseif section = "form">
        <div class="fitflow-page-actions">
            <p class="fitflow-expired-copy">
                Начните вход заново или продолжите текущий процесс, если страница еще доступна.
            </p>
            <a id="loginRestartLink" class="fitflow-button fitflow-button--primary" href="${url.loginRestartFlowUrl}">
                Начать заново
            </a>
            <a id="loginContinueLink" class="fitflow-link fitflow-link--center" href="${url.loginAction}">
                Продолжить вход
            </a>
        </div>
    </#if>
</@layout.registrationLayout>
