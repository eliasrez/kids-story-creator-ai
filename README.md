# Kids Story Creator 🦄📚

A fun, creative web app where kids can create AI-generated stories and illustrations based on their ideas and drawings.

## Features
- Choose a story category
- Enter ideas and characters
- Upload a drawing
- AI generates a story using Azure OpenAI GPT-4
- AI creates a related illustration using DALL·E 3

## Technologies
- React (Vite + Tailwind CSS)
- Azure OpenAI (GPT-4 & DALL·E 3)
- Azure Functions (.NET)
- GitHub Actions (CI/CD)

## Prerequisites
- Azure Subscription
- Azure OpenAI resource with GPT-4 and DALL·E deployed

## Setup Instructions
1. Clone the repo
2. Run `npm install` in `frontend/`
3. Run the app with `npm run dev`
4. Configure environment variables for Azure Functions:
   - `AZURE_OPENAI_KEY`
   - `AZURE_OPENAI_ENDPOINT`
5. Deploy using GitHub Actions or Azure CLI

## License
MIT


# to run azure function locally
- func start

## Setup Notes

- In Azure Portal, go to your Function App → Deployment Center → Get Publish Profile.
- Add it to GitHub repo secrets as:

- AZURE_FUNCTIONAPP_PUBLISH_PROFILE

- Replace YOUR_FUNCTIONAPP_NAME with your actual Azure Function App name.

- If you want staging/prod environments, you can make separate workflows or jobs.