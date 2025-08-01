#!/bin/bash

RESOURCE_GROUP="kids-story-rg"
LOCATION="eastus"
STORAGE_ACCOUNT="kidsstory${RANDOM}"
FUNCTION_APP="kids-story-func"
PLAN="kids-story-plan"
AI_NAME="kids-story-ai"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create storage account
az storage account create --name $STORAGE_ACCOUNT --location $LOCATION --resource-group $RESOURCE_GROUP --sku Standard_LRS

# Create function app hosting plan
az functionapp plan create --name $PLAN --resource-group $RESOURCE_GROUP --location $LOCATION --number-of-workers 1 --sku Y1 --is-linux

# Create function app
az functionapp create --name $FUNCTION_APP --storage-account $STORAGE_ACCOUNT --resource-group $RESOURCE_GROUP \
  --plan $PLAN --runtime dotnet --functions-version 4 --os-type Linux

# Create OpenAI resource
az cognitiveservices account create --name $AI_NAME --resource-group $RESOURCE_GROUP --location $LOCATION \
  --kind OpenAI --sku S0 --yes --custom-domain kidsstoryopenai

echo "✅ Resources created."
echo "👉 Next: Deploy GPT-4 and DALL·E 3 via Azure Portal (Models tab)."
