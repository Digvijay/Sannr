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
        ['link', { rel: 'icon', href: '/icon.png' }]
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
