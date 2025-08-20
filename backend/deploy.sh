#!/bin/bash

# ----------- CONFIG -----------
RESOURCE_GROUP="kids-story-rg"
LOCATION="eastus"
OPENAI_NAME="kids-openai"
STORAGE_NAME="kidsstorysa$RANDOM"
FUNCTION_APP_NAME="kids-story-func$RANDOM"
#FUNCTION_APP_NAME="kids-story-func17607"
PLAN_NAME="kids-story-plan"
RUNTIME="dotnet-isolated"
GPT_DEPLOY_NAME="gpt-4"
DALLE_DEPLOY_NAME="dall-e-3"
# ------------------------------

echo "✅ Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

echo "✅ Creating Azure OpenAI resource in $LOCATION..."
az cognitiveservices account create \
  --name $OPENAI_NAME \
  --resource-group $RESOURCE_GROUP \
  --kind OpenAI \
  --sku S0 \
  --location $LOCATION \
  --yes

echo "✅ Deploying GPT-4 model..."
az cognitiveservices account deployment create \
  --resource-group $RESOURCE_GROUP \
  --name $OPENAI_NAME \
  --deployment-name $GPT_DEPLOY_NAME \
  --model-name "gpt-4.1" \
  --model-version "2025-04-14" \
  --model-format "OpenAI" \
  --sku-capacity 1 \
  --sku-name "GlobalStandard"

echo "✅ Deploying DALL·E-3 model..."
az cognitiveservices account deployment create \
  --resource-group $RESOURCE_GROUP \
  --name $OPENAI_NAME \
  --deployment-name $DALLE_DEPLOY_NAME \
  --model-name "dall-e-3" \
  --model-version "3.0" \
  --model-format "OpenAI" \
  --sku-capacity 1 \
  --sku-name "Standard"

echo "✅ Creating Storage Account..."
az storage account create \
  --name $STORAGE_NAME \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --sku Standard_LRS

echo "✅ Creating App Service Plan..."
az functionapp plan create \
  --name $PLAN_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku F1 \
  --is-linux

echo "✅ Creating Azure Function App..."
az functionapp create \
  --name $FUNCTION_APP_NAME \
  --storage-account "kidsstorysa10680" \
  --resource-group $RESOURCE_GROUP \
  --plan $PLAN_NAME \
  --runtime $RUNTIME \
  --functions-version 4 \
  --os-type Linux

echo "🔐 Fetching OpenAI Key..."
OPENAI_KEY=$(az cognitiveservices account keys list --name $OPENAI_NAME --resource-group $RESOURCE_GROUP --query key1 -o tsv)

echo "✅ Configuring App Settings..."
az functionapp config appsettings set \
  --name $FUNCTION_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings "AZURE_OPENAI_ENDPOINT=https://$OPENAI_NAME.openai.azure.com/" \
             "AZURE_OPENAI_KEY=$OPENAI_KEY" \
             "GPT_DEPLOYMENT_NAME=$GPT_DEPLOY_NAME" \
             "DALLE_DEPLOYMENT_NAME=$DALLE_DEPLOY_NAME"

echo "✅ Publishing Azure Function..."
cd azure-function/
func azure functionapp publish $FUNCTION_APP_NAME --force

echo "🎉 Deployment complete! Azure Function App: https://$FUNCTION_APP_NAME.azurewebsites.net"
