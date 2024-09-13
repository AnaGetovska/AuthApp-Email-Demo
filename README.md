# Two-Factor Email Authentication App
Basic app providing simple functionality to demo Email-based two-factor authentication (2FA).

## Requirements
- Install [SQL Express](https://www.microsoft.com/en-us/download/details.aspx?id=101064)
- [SendGrid account](https://sendgrid.com/en-us)

## SendGrid Setup Instructions

To enable email functionality in this application, you need to configure SendGrid by following the steps below:

### Step 1: Register with SendGrid
1. Go to [SendGrid's website](https://sendgrid.com/en-us) and create an account if you haven't already.
2. Complete the registration process and log into your SendGrid dashboard.

### Step 2: Generate API Key
1. From the SendGrid dashboard, navigate to the **left-hand sidebar** and select **Email API**.
2. Under **Email API**, click on **Integration Guide** to set up your email sending configuration.
3. Follow the steps in the guide to generate a new **API Key**.

### Step 3: Store SendGrid Credentials
After generating your SendGrid API Key, you will need to securely store the following details in the project configuration:
- **SendGridKey**: The API Key generated from the SendGrid dashboard.
- **From**: The email address that will be used to send emails from your application.
- **Name**: The display name associated with the **From** email address (e.g., "My App Support").

It is recommended to store these sensitive credentials in a `secret.json` file to keep them secure and out of your source code.


