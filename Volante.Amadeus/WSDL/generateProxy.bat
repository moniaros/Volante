del ../AmadeusWebServices.cs
del ../output.config
svcutil 1ASIWESKESK_PRD_24July12.wsdl *.xsd /out:../AmadeusProxy.cs /config:../AmadeusProxy.config /language:C# /async