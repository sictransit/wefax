# wefax

This is code to generate a WeFax, Weather Fax, Radio Fax signal from an image. It is not a complete fax program, nor is it a fax receiver/decoder. 

It just scans pixels, transforming to a signal with a frequency shift. Optionally it supports my take on BCH (Binary Coded Header), as deduced from some very vague docs I found somewhere.

## features

None! I wrote this for an amateur radio Geocaching project, but feel free to fork or submit a pull request.

## tests

None, but at least [fldigi](http://www.w1hkj.com/) is able to decode the generated audio file, to something that looks very similar to the encoded image.
