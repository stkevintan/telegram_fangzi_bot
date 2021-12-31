#!/usr/bin/env pwsh

$ResourceUrl = "https://raw.githubusercontent.com/nagadomi/lbpcascade_animeface/master/lbpcascade_animeface.xml"

Invoke-WebRequest -Uri $ResourceUrl -OutFile lbpcascade_animeface.xml