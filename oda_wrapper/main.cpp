#include "OdaCommon.h"
#include "RxInit.h"
#include "RxDynamicModule.h"
#include "RxSystemServices.h"
#include "DbHostApp.h"
#include "DbDatabase.h"
#include "PdfExport.h"

#include <iostream>

class MyHostApp : public OdDbHostAppServices
{
public:
    const OdString program() const override { return OD_T("CloudmindConverter"); }
    const OdString product() const override { return OD_T("Cloudmind DWG to PDF"); }
};

int main(int argc, char* argv[])
{
    if (argc < 3)
    {
        std::cout << "Usage: odpdf <input.dwg> <output.pdf>\n";
        return 1;
    }

    OdString input = OdString(argv[1]);
    OdString output = OdString(argv[2]);

    try
    {
        // Init ODA system
        odInitialize();

        // Load PDF export module
        OdRxModulePtr pdfModule = ::odrxDynamicLinker()->loadModule(OdPdfExportModuleName, false);
        if (pdfModule.isNull())
        {
            std::cerr << "Failed to load ODA PDF export module.\n";
            return 2;
        }

        // Create host app
        OdDbHostAppServicesPtr hostApp = OdDbHostAppServicesPtr(new MyHostApp(), kOdRxObjAttach);

        // Read the DWG
        OdDbDatabasePtr db = hostApp->readFile(input);
        if (db.isNull())
        {
            std::cerr << "Unable to open DWG file.\n";
            return 3;
        }

        // Prepare PDF export parameters
        OdPdfExportPtr exporter = pdfModule->create();
        OdPdfExportParams params;

        params.setDatabase(db);
        params.setOutput(output);
        params.setExportAsOdaPdf(true);

        params.setBackground(ODRGB(255, 255, 255)); // white

        // Execute export
        if (exporter->exportPdf(params) != ::eOk)
        {
            std::cerr << "PDF export failed.\n";
            return 4;
        }

        std::cout << "PDF successfully created: " << output << "\n";
    }
    catch (const OdError& err)
    {
        std::cerr << "ODA Error: " << err.description().c_str() << "\n";
        return 5;
    }

    return 0;
}
