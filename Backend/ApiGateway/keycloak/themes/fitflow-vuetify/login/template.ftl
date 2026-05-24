<#macro registrationLayout bodyClass="" displayInfo=false displayMessage=true displayRequiredFields=false>
<!DOCTYPE html>
<html lang="${lang}"<#if realm.internationalizationEnabled> dir="${(locale.rtl)?then('rtl','ltr')}"</#if>>
<head>
    <meta charset="utf-8">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <meta name="robots" content="noindex, nofollow">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>${msg("loginTitle",(realm.displayName!''))}</title>
    <link rel="icon" href="${url.resourcesPath}/img/logo.svg">
    <#if properties.stylesCommon?has_content>
        <#list properties.stylesCommon?split(' ') as style>
            <link href="${url.resourcesCommonPath}/${style}" rel="stylesheet">
        </#list>
    </#if>
    <#if properties.styles?has_content>
        <#list properties.styles?split(' ') as style>
            <link href="${url.resourcesPath}/${style}" rel="stylesheet">
        </#list>
    </#if>
    <#if properties.scripts?has_content>
        <#list properties.scripts?split(' ') as script>
            <script src="${url.resourcesPath}/${script}" type="text/javascript"></script>
        </#list>
    </#if>
    <#if scripts??>
        <#list scripts as script>
            <script src="${script}" type="text/javascript"></script>
        </#list>
    </#if>
    <#if authenticationSession??>
        <script type="module">
            import { checkAuthSession } from "${url.resourcesPath}/js/authChecker.js";
            checkAuthSession("${authenticationSession.authSessionIdHash}");
        </script>
    </#if>
</head>
<body class="fitflow-auth-body ${bodyClass}" data-page-id="login-${pageId!'auth'}">
    <div class="fitflow-auth-page">
        <a class="fitflow-brand" href="${properties.fitflowLandingUrl!'http://127.0.0.1:5173/landing'}" aria-label="FitFlow">
            <span class="fitflow-brand__mark">
                <img src="${url.resourcesPath}/img/logo.svg" alt="">
            </span>
            <span class="fitflow-brand__text">fitflow.art</span>
        </a>

        <div class="fitflow-auth-stack">
            <main class="fitflow-card" aria-labelledby="kc-page-title">
                <#nested "show-username">

                <header class="fitflow-card__header">
                    <h1 id="kc-page-title" class="fitflow-title"><#nested "header"></h1>
                </header>

                <#if displayMessage && message?has_content && (message.type != 'warning' || !isAppInitiatedAction??)>
                    <div class="fitflow-alert fitflow-alert--${message.type}" role="alert">
                        <span>${kcSanitize(message.summary)?no_esc}</span>
                    </div>
                </#if>

                <div id="kc-content" class="fitflow-card__content">
                    <div id="kc-content-wrapper">
                        <#nested "form">

                        <#if auth?has_content && auth.showTryAnotherWayLink()>
                            <form id="kc-select-try-another-way-form" action="${url.loginAction}" method="post" class="fitflow-form">
                                <input type="hidden" name="tryAnotherWay" value="on">
                                <button type="submit" class="fitflow-link-button">${msg("doTryAnotherWay")}</button>
                            </form>
                        </#if>

                        <#nested "socialProviders">

                        <#if displayInfo>
                            <div id="kc-info" class="fitflow-info">
                                <#nested "info">
                            </div>
                        </#if>
                    </div>
                </div>
            </main>
        </div>
    </div>
</body>
</html>
</#macro>
