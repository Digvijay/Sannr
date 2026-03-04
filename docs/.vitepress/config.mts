import { defineConfig } from 'vitepress'
import { createRequire } from 'module'
const require = createRequire(import.meta.url)
const pkg = require('../package.json')

export default defineConfig({
    srcExclude: ['[A-Z]*.md'],
    base: '/',
    title: "Sannr",
    description: "Enterprise-Grade, AOT-First Validation Engine for .NET",
    head: [
        ['link', { rel: 'icon', href: '/icon.png' }],
        ['script', {}, `
            (function() {
                var cookieName = 'sannr-cookie-consent';
                if (!localStorage.getItem(cookieName)) {
                    document.addEventListener('DOMContentLoaded', function() {
                        var banner = document.createElement('div');
                        banner.id = 'cookie-banner';
                        banner.style.position = 'fixed';
                        banner.style.bottom = '20px';
                        banner.style.left = '50%';
                        banner.style.transform = 'translateX(-50%)';
                        banner.style.backgroundColor = 'var(--vp-c-bg-soft)';
                        banner.style.color = 'var(--vp-c-text-1)';
                        banner.style.padding = '12px 24px';
                        banner.style.borderRadius = '12px';
                        banner.style.boxShadow = '0 8px 24px rgba(0,0,0,0.15)';
                        banner.style.zIndex = '1000';
                        banner.style.display = 'flex';
                        banner.style.alignItems = 'center';
                        banner.style.gap = '16px';
                        banner.style.fontSize = '14px';
                        banner.style.border = '1px solid var(--vp-c-divider)';
                        banner.style.backdropFilter = 'blur(8px)';
                        
                        banner.innerHTML = '<span>We use cookies to improve your experience.</span>' +
                                         '<button id="accept-cookies" style="background: var(--vp-c-brand); color: white; border: none; padding: 6px 16px; borderRadius: 8px; cursor: pointer; fontWeight: 600;">Accept</button>';
                        
                        document.body.appendChild(banner);
                        
                        document.getElementById('accept-cookies').onclick = function() {
                            localStorage.setItem(cookieName, 'true');
                            banner.style.display = 'none';
                        };
                    });
                }
            })();
        `]
    ],
    themeConfig: {
        nav: [
            { text: `v${pkg.version}`, link: 'https://github.com/Digvijay/Sannr/releases', target: '_blank' },
            { text: 'Home', link: '/' },
            { text: 'Guide', link: '/guide/getting-started' },
            { text: 'Features', link: '/features/validation-attributes' },
            { text: 'Concepts', link: '/concepts/architecture' },
            { text: 'Reference', link: '/api/index' }
        ],

        sidebar: [
            {
                text: 'Introduction',
                items: [
                    { text: 'Getting Started', link: '/guide/getting-started' },
                    { text: 'Architecture', link: '/concepts/architecture' },
                    { text: 'Migration Guide', link: '/guide/migration' },
                    { text: 'Sannr vs Others', link: '/guide/comparison' },
                    { text: 'Roadmap', link: '/roadmap' }
                ]
            },
            {
                text: 'Core Validation',
                items: [
                    { text: 'Validation Attributes', link: '/features/validation-attributes' },
                    { text: 'Sanitization', link: '/features/sanitization' },
                    { text: 'Conditional Validation', link: '/features/conditional-validation' },
                    { text: 'Custom Validators', link: '/features/custom-validators' },
                    { text: 'Async Validation', link: '/features/async-validation' },
                    { text: 'Validation Groups', link: '/features/validation-groups' },
                    { text: 'Model-Level Validation', link: '/features/model-level-validation' },
                    { text: 'Business Rule Validators', link: '/features/business-rules' }
                ]
            },
            {
                text: 'Integrations',
                items: [
                    { text: 'ASP.NET Core', link: '/integrations/aspnet-core' },
                    { text: 'Minimal APIs', link: '/integrations/minimal-apis' },
                    { text: 'OpenAPI / Swagger', link: '/integrations/openapi' },
                    { text: 'Client-Side Validation', link: '/integrations/client-side' },
                    { text: 'Performance Monitoring', link: '/integrations/monitoring' }
                ]
            },
            {
                text: 'Static Reflection',
                items: [
                    { text: 'Shadow Types', link: '/features/shadow-types' },
                    { text: 'PII Awareness', link: '/features/pii' },
                    { text: 'Deep Cloning', link: '/features/deep-cloning' }
                ]
            },
            {
                text: 'Concepts',
                items: [
                    { text: 'How It Works', link: '/concepts/architecture' },
                    { text: 'Performance', link: '/concepts/performance' },
                    { text: 'Technical Summary', link: '/concepts/technical-summary' },
                    { text: 'Executive Summary', link: '/concepts/executive-summary' }
                ]
            },
            {
                text: 'API Reference',
                items: [
                    { text: 'API Overview', link: '/api/index' },
                    { text: 'Attributes Reference', link: '/api/attributes' },
                    { text: 'Configuration', link: '/api/configuration' },
                    { text: 'CLI Reference', link: '/api/cli' },
                    { text: 'Troubleshooting', link: '/api/troubleshooting' }
                ]
            },
            {
                text: 'Resources',
                items: [
                    { text: 'Changelog', link: '/changelog' },
                    { text: 'Limitations', link: '/limitations' },
                    { text: 'Contributing', link: '/contributing' },
                    { text: 'Security', link: '/security' }
                ]
            }
        ],

        socialLinks: [
            { icon: 'github', link: 'https://github.com/Digvijay/Sannr' }
        ],

        footer: {
            message: 'Released under the MIT License.',
            copyright: 'Copyright © 2026 Digvijay Chauhan'
        }
    }
})
