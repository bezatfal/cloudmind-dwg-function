# 📐 Cloudmind DWG Function

### Azure Function for Processing DWG Files and Generating Metadata

This project is an **Azure Functions Isolated Worker (.NET 8)** service
designed to process DWG files uploaded to Azure Blob Storage. When a DWG
file is added to the monitored input container, the function extracts
basic metadata (or future enriched metadata) and stores the results in
an output container. If processing fails, the function automatically
routes the original file to an error container.

## 🚀 Features

-   **Blob-triggered DWG processing**
-   **Generates JSON metadata** for each DWG file
-   **Automatic error handling & file routing**
-   **Supports Azure.Identity (Managed Identity)**
-   **Isolated Worker Model (dotnet-isolated)**
-   **Clean, production-ready .NET 8 implementation**
-   **Easy to deploy via GitHub Actions or Azure CLI**

## 🧱 Architecture

    DWG Input Container  →  Function Triggered  →  Process DWG  →  Output Metadata JSON
                                   ↓
                            On Error → Error Container

## 📁 Project Structure

    cloudmind-dwg-function/
    │
    ├── DwgProcessor.cs        # Main DWG processing function
    ├── Program.cs             # Worker host builder
    ├── host.json              # Function host configuration
    ├── local.settings.json    # Local dev settings (ignored in Git)
    ├── cloudmind-dwg-function.csproj
    └── cloudmind-dwg-function.sln

## ⚙️ Configuration

### Required Environment Variables

  Variable Name            Description
  ------------------------ --------------------------------------------
  `STORAGE_ACCOUNT_NAME`   Azure Storage account name
  `DWG_INPUT_CONTAINER`    Input container for DWG uploads
  `DWG_OUTPUT_CONTAINER`   Output container for metadata JSON
  `DWG_ERROR_CONTAINER`    Error container for failed DWG files
  `AzureWebJobsStorage`    Storage connection string (local use only)

## 🛠️ Local Development

### 1. Install Azure Functions Core Tools

Verify installation:

    func --version

### 2. Restore dependencies

    dotnet restore

### 3. Run locally

    func start

### 4. Test locally

Upload a `.dwg` file into your input container.

## 🏗️ Deployment

You may deploy using Azure Portal, Azure CLI, or GitHub Actions.

## 🧩 Future Enhancements

-   Full DWG parsing pipeline\
-   Extract title block metadata\
-   Multi-page drawing analysis\
-   Integration with Cloudmind AI pipeline\
-   Application Insights telemetry

## 📄 License

Internal Cloudmind Consultants project.
