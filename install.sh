#!/bin/bash
if [ $DOTNET = "true" ]; then
	dotnet restore
fi

if [ $MONO = "true" ]; then
	nuget restore NtfsSharp.sln
	nuget install NUnit.ConsoleRunner -Version 3.6.1 -OutputDirectory testrunner
fi