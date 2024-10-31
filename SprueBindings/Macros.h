#pragma once

#define STRING_TO_CHAR(SRC) (const char*)(Marshal::StringToHGlobalAnsi(SRC)).ToPointer();