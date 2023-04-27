terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.53.0"
    }
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {}
}

resource "azurerm_service_plan" "resume_service_plan"{
    name                            = "resume-service-plan"
    resource_group_name             = var.resource_group
    location                        = "eastus"
    os_type                         = "Windows"
    sku_name                        = "B1"

}
    
resource "azurerm_windows_function_app" "resume_view_count_function_app"{
    name                            = "resume-view-count-function"
    resource_group_name             = var.resource_group
    location                        = var.region
    storage_account_name            = var.storage_account_name
    storage_account_access_key      = var.storage_account_access_key
    service_plan_id                 = azurerm_service_plan.resume_service_plan.id
    
    site_config {
      always_on                     = "false"
    }
}

resource "azurerm_function_app_function" "get_resume_views_function"{
    name                            = "get-resume-views-function"
    function_app_id                 = azurerm_windows_function_app.resume_view_count_function_app.id
    language                        = "CSharp"
    config_json                     = jsonencode({
        "bindings" = [{
            "authlevel" = "function"
            "direciton" = "in"
            "methods" = [
                "get",
                "post",
            ]
            "name" = "get_views"
            "type" = "httpTrigger"
            }]
    })
}

resource "azurerm_cosmosdb_account" "resume-website-db"{
    name                            = "resume-website-db"
    location                        = var.region
    resource_group_name             = var.resource_group
    offer_type                      = "Standard"
    enable_free_tier                = "true"

    consistency_policy{
        consistency_level           = "BoundedStaleness"
    }

    geo_location{
        location                    = var.region
        failover_priority           = 0
    }

    capabilities{
        name                        = "EnableTable"
    }
}

resource "azurerm_cosmosdb_table" "resume-table"{
        name                        = "resume-view-table"
        resource_group_name         = var.resource_group
        account_name                = azurerm_cosmosdb_account.resume-website-db.name
        throughput                  = "400"
}