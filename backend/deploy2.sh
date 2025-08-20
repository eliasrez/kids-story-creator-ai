#!/bin/bash
# To purge deleted resources
#elias@DESKTOP-70LA0RV MINGW64 /c/AI/kids-story-creator/backend (master)
#$ az cognitiveservices account list-deleted -o table
#Kind        Location       Name
#----------  -------------  ----------------------------
#AIServices  swedencentral  elias-mdxi4e6y-swedencentral
#
#elias@DESKTOP-70LA0RV MINGW64 /c/AI/kids-story-creator/backend (master)
#$ az cognitiveservices account purge -g kids-stories-rg -l swedencentral  --name elias-mdxi4e6y-swedencentral


# ----------- CONFIG -----------
RESOURCE_GROUP="kids-story-rg"
LOCATION="eastus"
OPENAI_NAME="kids-openai"
STORAGE_NAME="kidsstorysa$RANDOM"
FUNCTION_APP_NAME="kids-story-func$RANDOM"
PLAN_NAME="kids-story-plan"
RUNTIME="dotnet-isolated"
GPT_DEPLOY_NAME="gpt-4"
DALLE_DEPLOY_NAME="dall-e-3"
# ------------------------------

#echo "✅ Creating resource group..."
#az group create --name $RESOURCE_GROUP --location $LOCATION

echo "✅ Creating Azure OpenAI resource in $LOCATION..."
az cognitiveservices account create \
  --name $OPENAI_NAME \
  --resource-group $RESOURCE_GROUP \
  --kind OpenAI \
  --sku S0 \
  --location $LOCATION \
  --yes