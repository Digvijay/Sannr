// Sannr Validation Demo JavaScript

// Using ApiService with attribute-based validation
const currentApiService = 'apiservice';

// Tab switching functionality
function showTab(tabName) {
    // Hide all tabs
    const tabs = document.querySelectorAll('.tab-content');
    tabs.forEach(tab => tab.classList.remove('active'));

    // Remove active class from all buttons
    const buttons = document.querySelectorAll('.tab-button');
    buttons.forEach(button => button.classList.remove('active'));

    // Show selected tab
    document.getElementById(tabName).classList.add('active');

    // Add active class to clicked button
    if (event && event.target) {
        event.target.classList.add('active');
    }
}

function showSubTab(subTabName) {
    // Hide all sub-tabs
    const subTabs = document.querySelectorAll('.sub-tab-content');
    subTabs.forEach(tab => tab.classList.remove('active'));

    // Remove active class from all sub-buttons
    const buttons = document.querySelectorAll('.sub-tab-button');
    buttons.forEach(button => button.classList.remove('active'));

    // Show selected sub-tab
    document.getElementById(subTabName).classList.add('active');

    // Add active class to clicked button
    if (event && event.target) {
        event.target.classList.add('active');
    }
}

// Form submission handling
async function submitForm(event, endpoint) {
    event.preventDefault();

    const form = event.target;
    const formData = new FormData(form);
    const data = {};

    // Capture all form elements to ensure checkboxes and other fields are captured correctly
    Array.from(form.elements).forEach(element => {
        if (!element.name) return;

        if (element.type === 'checkbox') {
            data[element.name] = element.checked;
        } else if (element.type === 'radio') {
            if (element.checked) data[element.name] = element.value;
        } else if (element.type === 'number') {
            data[element.name] = element.value ? parseFloat(element.value) : null;
        } else {
            data[element.name] = element.value;
        }
    });

    // Handle datetime-local inputs
    form.querySelectorAll('input[type="datetime-local"]').forEach(input => {
        if (input.value) {
            data[input.name] = input.value;
        }
    });

    const resultContainer = document.getElementById(`${endpoint}-result`) || document.getElementById('business-rules-result');
    resultContainer.innerHTML = '<div class="loading"></div>Submitting...';

    try {
        // Use the web app's proxy endpoints instead of direct service calls
        let endpointPath = '/api/test/'; // Default to attribute-based service

        if (currentApiService === 'fluentapiservice') {
            endpointPath = '/api/fluent/'; // Use fluent service proxy
        }

        const url = `${endpointPath}${endpoint}`;
        console.log(url);
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });

        // Some responses might be empty or non-JSON; guard against malformed JSON
        const text = await response.text();
        const result = text ? JSON.parse(text) : { message: 'No content', data: null };

        if (response.ok) {
            displaySuccess(result, resultContainer, endpoint);
        } else {
            displayErrors(result, resultContainer);
        }
    } catch (error) {
        resultContainer.innerHTML = `<div class="error-item">Error: ${error.message}</div>`;
    }
}

function displaySuccess(result, container, endpoint) {
    container.style.display = 'block';

    let content = `
        <div class="success-header">
            <svg style="width:24px;height:24px" viewBox="0 0 24 24">
                <path fill="currentColor" d="M21,7L9,19L3.5,13.5L4.91,12.09L9,16.17L19.59,5.59L21,7Z" />
            </svg>
            <span>${result.message || 'Success!'}</span>
        </div>
    `;

    if (result.sanitizedData) {
        content += `
            <h4>Sanitized Data:</h4>
            <pre>${JSON.stringify(result.sanitizedData, null, 2)}</pre>
        `;
    }

    if (result.profile) {
        content += `
            <h4>Profile:</h4>
            <pre>${JSON.stringify(result.profile, null, 2)}</pre>
        `;
    }

    if (result.order) {
        content += `
            <h4>Order Details:</h4>
            <pre>${JSON.stringify(result.order, null, 2)}</pre>
        `;
    }

    if (result.product) {
        content += `
            <h4>Product Details:</h4>
            <pre>${JSON.stringify(result.product, null, 2)}</pre>
        `;
    }

    if (result.appointment) {
        content += `
            <h4>Appointment Details:</h4>
            <pre>${JSON.stringify(result.appointment, null, 2)}</pre>
        `;
    }

    if (result.data) {
        content += `
            <h4>Validated Data:</h4>
            <pre>${JSON.stringify(result.data, null, 2)}</pre>
        `;
    }

    if (result.securityCode !== undefined) {
        content += `<p><strong>Security Code:</strong> ${result.securityCode}</p>`;
    }

    container.innerHTML = content;
}

function displayErrors(result, container) {
    container.style.display = 'block';

    let errorsHtml = '';
    if (result.errors) {
        for (const [field, messages] of Object.entries(result.errors)) {
            messages.forEach(msg => {
                errorsHtml += `<div class="error-item"><strong>${field}:</strong> ${msg}</div>`;
            });
        }
    } else {
        errorsHtml = `<div class="error-item">${result.title || 'Validation failed'}</div>`;
    }

    container.innerHTML = `
        <div class="error-header">
            <svg style="width:24px;height:24px" viewBox="0 0 24 24">
                <path fill="currentColor" d="M12,2L1,21H23L12,2M12,6L19.53,19H4.47L12,6M11,10V14H13V10H11M11,16V18H13V16H11Z" />
            </svg>
            <span>Validation Errors</span>
        </div>
        ${errorsHtml}
    `;
}

// Client-side validation logic using Sannr generated metadata
let clientRules = null;

async function loadClientRules() {
    try {
        const response = await fetch('/api/test/client-validation/rules');
        const code = await response.text();
        document.getElementById('client-generated-code').textContent = code;

        // Extract the object from the generated JS string
        // The generator produces: const clientValidationModelValidators = { ... };
        const jsonMatch = code.match(/\{[\s\S]*\}/);
        if (jsonMatch) {
            // Convert JS object string to usable object (safely)
            // Sannr generates: { prop: { rule: val }, ... }
            // We need to parse this. Since it's a simple object structure, we can use eval or a better parser.
            // For the demo, we'll use a simple regex-based conversion to JSON if possible, or eval.
            // Sannr's JS output is actually valid JS code.
            const objCode = jsonMatch[0];
            clientRules = eval('(' + objCode + ')');
        }
    } catch (e) {
        console.error('Failed to load client rules:', e);
        document.getElementById('client-generated-code').textContent = '// Error loading rules';
    }
}

function applyClientValidation() {
    if (!clientRules) return;

    const form = document.getElementById('client-form');
    document.getElementById('client-validation-message').style.display = 'block';

    const inputs = form.querySelectorAll('input');
    inputs.forEach(input => {
        input.addEventListener('input', () => {
            const field = input.name;
            const rules = clientRules[field];
            if (!rules) return;

            let error = null;
            const val = input.type === 'checkbox' ? input.checked : input.value;

            if (rules.required && !val) {
                error = 'This field is required';
            } else if (rules.email && val && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val)) {
                error = 'Invalid email address';
            } else if (rules.min !== undefined && val && parseFloat(val) < rules.min) {
                error = `Value must be at least ${rules.min}`;
            } else if (rules.max !== undefined && val && parseFloat(val) > rules.max) {
                error = `Value must be at most ${rules.max}`;
            } else if (rules.maxLength !== undefined && val && val.length > rules.maxLength) {
                error = `Maximum length is ${rules.maxLength}`;
            } else if (rules.requiredIf) {
                const otherVal = form.querySelector(`[name="${rules.requiredIf.otherProperty}"]`).checked;
                if (otherVal === rules.requiredIf.targetValue && !val) {
                    error = 'This field is required based on choice';
                }
            } else if (rules.allowedValues && val && !rules.allowedValues.includes(val)) {
                error = `Must be one of: ${rules.allowedValues.join(', ')}`;
            }

            // Simple visual feedback
            if (error) {
                input.style.borderColor = '#e06c75';
                let errorTag = input.parentNode.querySelector('.client-error');
                if (!errorTag) {
                    errorTag = document.createElement('div');
                    errorTag.className = 'client-error';
                    errorTag.style.color = '#e06c75';
                    errorTag.style.fontSize = '0.8em';
                    errorTag.style.marginTop = '2px';
                    input.parentNode.appendChild(errorTag);
                }
                errorTag.textContent = error;
            } else {
                input.style.borderColor = '';
                const errorTag = input.parentNode.querySelector('.client-error');
                if (errorTag) errorTag.remove();
            }
        });
    });
}

// Initial page setup
window.addEventListener('DOMContentLoaded', () => {
    // Current date logic
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 7);
    const dateString = futureDate.toISOString().slice(0, 16);

    const dateInputs = document.querySelectorAll('input[type="datetime-local"]');
    dateInputs.forEach(input => {
        if (!input.value) {
            input.value = dateString;
        }
    });

    // Pre-fill test data
    setTimeout(() => {
        const getEl = (id) => document.getElementById(id);
        const setVal = (id, val) => { const el = getEl(id); if (el) el.value = val; };

        setVal('comp-username', 'testuser');
        setVal('comp-displayname', 'ValidName');
        setVal('comp-age', '25');
        setVal('comp-price', '99.99');
        setVal('comp-email', 'test@example.com');
        setVal('comp-userid', '  john_doe  ');

        setVal('profile-username', '  jane_smith  ');
        setVal('profile-email', 'jane@example.com');
        setVal('profile-age', '30');
        setVal('profile-country', 'USA');
        setVal('profile-zipcode', '90210');

        setVal('adv-username', 'advanced_user');
        setVal('adv-email', 'adv@example.com');
        setVal('adv-password', 'StrongPass123!');
        setVal('adv-security-code', '1234');

        setVal('order-customer-id', 'CUST-12345');
        setVal('order-currency', 'USD');
        setVal('order-amount', '500');

        setVal('product-name', 'New Product');
        setVal('product-status', 'active');
        setVal('product-price', '199.99');

        setVal('appt-customer-name', 'John Doe');
        setVal('appt-status', 'confirmed');
        setVal('appt-duration', '120');

        setVal('client-name', 'Client User');
        setVal('client-email', 'client@example.com');
        setVal('client-age', '28');
        const hasAddrEle = getEl('client-has-address');
        if (hasAddrEle) hasAddrEle.checked = true;
        setVal('client-street', '123 Main St');
    }, 100);

    loadClientRules();
});