// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#include <stdio.h>
#include <tchar.h>



// TODO: reference additional headers your program requires here

#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <map>
#include <algorithm>


 #ifndef UNICODE  
	typedef std::string String;
 #else
	typedef std::wstring String;
 #endif

using namespace std;
