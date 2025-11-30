cmake_minimum_required(VERSION 3.10)
project(odpdf)

set(CMAKE_CXX_STANDARD 17)

# Path to the ODA SDK libs
set(ODA_DIR "../oda")

include_directories(${ODA_DIR})
link_directories(${ODA_DIR})

add_executable(odpdf main.cpp)

target_link_libraries(odpdf
    TD_Db
    TD_DbCore
    TD_Ge
    TD_Root
    TD_PdfExport
    TD_PdfHooks
    TD_Gi
    TD_Alloc
    TD_Zlib
    libcrypto
    libssl
)
