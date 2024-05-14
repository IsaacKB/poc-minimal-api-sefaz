﻿using ApiNFe.Services.Interfaces;
using AutoMapper;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace ApiNFe.Services
{
    public class SoapService : ISoapService
    {
        private readonly IMapper _mapper;

        public SoapService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IResult execute()
        {
            string messageToRabbit = "TESTE";

            //_rabbitMQClient.sendMessage(messageToRabbit);

            return CallWebService();
        }

        private IResult CallWebService()
        {
            try
            {
                var _url = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx";
                var _action = "http://xxxxxxxx/Service1.asmx?op=HelloWorld";

                XmlDocument soapEnvelopeXml = CreateSoapEnvelope();
                X509Certificate2Collection certificates = CreateCertificates();
                HttpWebRequest webRequest = CreateWebRequest(_url, _action, certificates);
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                string soapResult;
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        soapResult = rd.ReadToEnd();
                    }

                    Console.WriteLine(soapResult);
                }

                XmlDocument xdoc = new XmlDocument();//xml doc used for xml parsing

                xdoc.LoadXml(soapResult);

                XmlNodeList cStatXmlNodeList = xdoc.GetElementsByTagName("cStat");
                string? codeResponse = cStatXmlNodeList[0]?.InnerXml;

                XmlNodeList xMotivoXmlNodeList = xdoc.GetElementsByTagName("xMotivo");
                string? xMotivo = xMotivoXmlNodeList[0]?.InnerXml;

                if (codeResponse != "103")
                {
                    return TypedResults.Problem(xMotivo);
                }

                return TypedResults.Ok(xMotivo);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                throw;
            }
        }

        private X509Certificate2Collection CreateCertificates()
        {
            string certName = @"C:\temp\cert.pfx";
            string password = @"rave23";

            X509Certificate2Collection certificates = new X509Certificate2Collection();

            certificates.Import(certName, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            return certificates;
        }

        private HttpWebRequest CreateWebRequest(string url, string action, X509Certificate2Collection certificates)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            //webRequest.Headers.Add("SOAPAction", action);
            webRequest.ClientCertificates = certificates;
            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            return webRequest;
        }

        private XmlDocument CreateSoapEnvelope()
        {
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(
                @"
                    <soap:Envelope 
                        xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
                        xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4"">
                        <soap:Header />
                        <soap:Body>
                            <nfe:nfeDadosMsg>
                                <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
                                    xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4"">
                                    <soap:Header />
                                    <soap:Body>
                                        <nfe:nfeDadosMsg>
                                            <NFe xmlns=""http://www.portalfiscal.inf.br/nfe"">
                                                <infNFe Id=""NFe35150300822602000124550010009923461099234656"" versao=""3.10"">
                                                    <ide>
                                                        <cUF>35</cUF>
                                                        <cNF>09923465</cNF>
                                                        <natOp>Venda prod. do estab.</natOp>
                                                        <indPag>1</indPag>
                                                        <mod>55</mod>
                                                        <serie>1</serie>
                                                        <nNF>992346</nNF>
                                                        <dhEmi>2015-03-27T09:40:00-03:00</dhEmi>
                                                        <dhSaiEnt>2015-03-27T09:40:00-03:00</dhSaiEnt>
                                                        <tpNF>1</tpNF>
                                                        <idDest>1</idDest>
                                                        <cMunFG>3550308</cMunFG>
                                                        <tpImp>1</tpImp>
                                                        <tpEmis>1</tpEmis>
                                                        <cDV>6</cDV>
                                                        <tpAmb>2</tpAmb>
                                                        <finNFe>1</finNFe>
                                                        <indFinal>1</indFinal>
                                                        <indPres>3</indPres>
                                                        <procEmi>3</procEmi>
                                                        <verProc>3.10.43</verProc>
                                                    </ide>
                                                    <emit>
                                                        <CNPJ>00822602000124</CNPJ>
                                                        <xNome>Plotag Sistemas e Suprimentos Ltda</xNome>
                                                        <xFant>Plotag - Localhost</xFant>
                                                        <enderEmit>
                                                            <xLgr>Rua Solon</xLgr>
                                                            <nro>558</nro>
                                                            <xBairro>Bom Retiro</xBairro>
                                                            <cMun>3550308</cMun>
                                                            <xMun>Sao Paulo</xMun>
                                                            <UF>SP</UF>
                                                            <CEP>01127010</CEP>
                                                            <cPais>1058</cPais>
                                                            <xPais>BRASIL</xPais>
                                                            <fone>1123587604</fone>
                                                        </enderEmit>
                                                        <IE>114489114119</IE>
                                                        <CRT>1</CRT>
                                                    </emit>
                                                    <dest>
                                                        <CNPJ>99999999000191</CNPJ>
                                                        <xNome>NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL</xNome>
                                                        <enderDest>
                                                            <xLgr>Rua Jaragua</xLgr>
                                                            <nro>774</nro>
                                                            <xBairro>Bom Retiro</xBairro>
                                                            <cMun>3550308</cMun>
                                                            <xMun>Sao Paulo</xMun>
                                                            <UF>SP</UF>
                                                            <CEP>01129000</CEP>
                                                            <cPais>1058</cPais>
                                                            <xPais>BRASIL</xPais>
                                                            <fone>33933501</fone>
                                                        </enderDest>
                                                        <indIEDest>9</indIEDest>
                                                        <email>gui_calabria@yahoo.com.br</email>
                                                    </dest>
                                                    <det nItem=""1"">
                                                        <prod>
                                                            <cProd>B17025056</cProd>
                                                            <cEAN />
                                                            <xProd>PAPEL MAXPLOT- 170MX250MX56GRS 3""</xProd>
                                                            <NCM>48025599</NCM>
                                                            <CFOP>5101</CFOP>
                                                            <uCom>Rl</uCom>
                                                            <qCom>1.0000</qCom>
                                                            <vUnCom>138.3000</vUnCom>
                                                            <vProd>138.30</vProd>
                                                            <cEANTrib />
                                                            <uTrib>RL</uTrib>
                                                            <qTrib>1.0000</qTrib>
                                                            <vUnTrib>138.3000</vUnTrib>
                                                            <indTot>1</indTot>
                                                        </prod>
                                                        <imposto>
                                                            <vTotTrib>41.49</vTotTrib>
                                                            <ICMS>
                                                                <ICMSSN101>
                                                                    <orig>0</orig>
                                                                    <CSOSN>101</CSOSN>
                                                                    <pCredSN>2.5600</pCredSN>
                                                                    <vCredICMSSN>3.54</vCredICMSSN>
                                                                </ICMSSN101>
                                                            </ICMS>
                                                            <IPI>
                                                                <clEnq>48025</clEnq>
                                                                <CNPJProd>00822602000124</CNPJProd>
                                                                <cEnq>599</cEnq>
                                                                <IPINT>
                                                                    <CST>53</CST>
                                                                </IPINT>
                                                            </IPI>
                                                            <PIS>
                                                                <PISNT>
                                                                    <CST>07</CST>
                                                                </PISNT>
                                                            </PIS>
                                                            <COFINS>
                                                                <COFINSNT>
                                                                    <CST>07</CST>
                                                                </COFINSNT>
                                                            </COFINS>
                                                        </imposto>
                                                    </det>
                                                    <det nItem=""2"">
                                                        <prod>
                                                            <cProd>1070100752</cProd>
                                                            <cEAN />
                                                            <xProd>PAPEL MAXPLOT- 1070X100MX75GRS 2""</xProd>
                                                            <NCM>48025599</NCM>
                                                            <CFOP>5101</CFOP>
                                                            <uCom>RL</uCom>
                                                            <qCom>1.0000</qCom>
                                                            <vUnCom>48.9100</vUnCom>
                                                            <vProd>48.91</vProd>
                                                            <cEANTrib />
                                                            <uTrib>RL</uTrib>
                                                            <qTrib>1.0000</qTrib>
                                                            <vUnTrib>48.9100</vUnTrib>
                                                            <indTot>1</indTot>
                                                        </prod>
                                                        <imposto>
                                                            <vTotTrib>14.67</vTotTrib>
                                                            <ICMS>
                                                                <ICMSSN101>
                                                                    <orig>0</orig>
                                                                    <CSOSN>101</CSOSN>
                                                                    <pCredSN>2.5600</pCredSN>
                                                                    <vCredICMSSN>1.25</vCredICMSSN>
                                                                </ICMSSN101>
                                                            </ICMS>
                                                            <IPI>
                                                                <clEnq>48025</clEnq>
                                                                <CNPJProd>00822602000124</CNPJProd>
                                                                <cEnq>599</cEnq>
                                                                <IPINT>
                                                                    <CST>53</CST>
                                                                </IPINT>
                                                            </IPI>
                                                            <PIS>
                                                                <PISNT>
                                                                    <CST>07</CST>
                                                                </PISNT>
                                                            </PIS>
                                                            <COFINS>
                                                                <COFINSNT>
                                                                    <CST>07</CST>
                                                                </COFINSNT>
                                                            </COFINS>
                                                        </imposto>
                                                    </det>
                                                    <det nItem=""3"">
                                                        <prod>
                                                            <cProd>B17025056</cProd>
                                                            <cEAN />
                                                            <xProd>PAPEL MAXPLOT- 170MX250MX56GRS 3""</xProd>
                                                            <NCM>48025599</NCM>
                                                            <CFOP>5101</CFOP>
                                                            <uCom>Rl</uCom>
                                                            <qCom>1.0000</qCom>
                                                            <vUnCom>138.3000</vUnCom>
                                                            <vProd>138.30</vProd>
                                                            <cEANTrib />
                                                            <uTrib>RL</uTrib>
                                                            <qTrib>1.0000</qTrib>
                                                            <vUnTrib>138.3000</vUnTrib>
                                                            <indTot>1</indTot>
                                                        </prod>
                                                        <imposto>
                                                            <vTotTrib>41.49</vTotTrib>
                                                            <ICMS>
                                                                <ICMSSN101>
                                                                    <orig>0</orig>
                                                                    <CSOSN>101</CSOSN>
                                                                    <pCredSN>2.5600</pCredSN>
                                                                    <vCredICMSSN>3.54</vCredICMSSN>
                                                                </ICMSSN101>
                                                            </ICMS>
                                                            <IPI>
                                                                <clEnq>48025</clEnq>
                                                                <CNPJProd>00822602000124</CNPJProd>
                                                                <cEnq>599</cEnq>
                                                                <IPINT>
                                                                    <CST>53</CST>
                                                                </IPINT>
                                                            </IPI>
                                                            <PIS>
                                                                <PISNT>
                                                                    <CST>07</CST>
                                                                </PISNT>
                                                            </PIS>
                                                            <COFINS>
                                                                <COFINSNT>
                                                                    <CST>07</CST>
                                                                </COFINSNT>
                                                            </COFINS>
                                                        </imposto>
                                                    </det>
                                                    <det nItem=""4"">
                                                        <prod>
                                                            <cProd>B17040056</cProd>
                                                            <cEAN />
                                                            <xProd>PAPEL MAXPLOT - 1.700X400MX 56 GRS 3""</xProd>
                                                            <NCM>48025599</NCM>
                                                            <CFOP>5101</CFOP>
                                                            <uCom>Rl</uCom>
                                                            <qCom>1.0000</qCom>
                                                            <vUnCom>214.5700</vUnCom>
                                                            <vProd>214.57</vProd>
                                                            <cEANTrib />
                                                            <uTrib>Rl</uTrib>
                                                            <qTrib>1.0000</qTrib>
                                                            <vUnTrib>214.5700</vUnTrib>
                                                            <indTot>1</indTot>
                                                        </prod>
                                                        <imposto>
                                                            <vTotTrib>64.37</vTotTrib>
                                                            <ICMS>
                                                                <ICMSSN101>
                                                                    <orig>0</orig>
                                                                    <CSOSN>101</CSOSN>
                                                                    <pCredSN>2.5600</pCredSN>
                                                                    <vCredICMSSN>5.49</vCredICMSSN>
                                                                </ICMSSN101>
                                                            </ICMS>
                                                            <IPI>
                                                                <clEnq>48025</clEnq>
                                                                <CNPJProd>00822602000124</CNPJProd>
                                                                <cEnq>599</cEnq>
                                                                <IPINT>
                                                                    <CST>53</CST>
                                                                </IPINT>
                                                            </IPI>
                                                            <PIS>
                                                                <PISNT>
                                                                    <CST>07</CST>
                                                                </PISNT>
                                                            </PIS>
                                                            <COFINS>
                                                                <COFINSNT>
                                                                    <CST>07</CST>
                                                                </COFINSNT>
                                                            </COFINS>
                                                        </imposto>
                                                    </det>
                                                    <det nItem=""5"">
                                                        <prod>
                                                            <cProd>B18525056</cProd>
                                                            <cEAN />
                                                            <xProd>PAPEL MAXPLOT-1.85MX250MX56GRS 3""</xProd>
                                                            <NCM>48025599</NCM>
                                                            <CFOP>5101</CFOP>
                                                            <uCom>Rl</uCom>
                                                            <qCom>1.0000</qCom>
                                                            <vUnCom>149.8300</vUnCom>
                                                            <vProd>149.83</vProd>
                                                            <cEANTrib />
                                                            <uTrib>RL</uTrib>
                                                            <qTrib>1.0000</qTrib>
                                                            <vUnTrib>149.8300</vUnTrib>
                                                            <indTot>1</indTot>
                                                        </prod>
                                                        <imposto>
                                                            <vTotTrib>44.95</vTotTrib>
                                                            <ICMS>
                                                                <ICMSSN101>
                                                                    <orig>0</orig>
                                                                    <CSOSN>101</CSOSN>
                                                                    <pCredSN>2.5600</pCredSN>
                                                                    <vCredICMSSN>3.84</vCredICMSSN>
                                                                </ICMSSN101>
                                                            </ICMS>
                                                            <IPI>
                                                                <clEnq>48025</clEnq>
                                                                <CNPJProd>00822602000124</CNPJProd>
                                                                <cEnq>599</cEnq>
                                                                <IPINT>
                                                                    <CST>53</CST>
                                                                </IPINT>
                                                            </IPI>
                                                            <PIS>
                                                                <PISNT>
                                                                    <CST>07</CST>
                                                                </PISNT>
                                                            </PIS>
                                                            <COFINS>
                                                                <COFINSNT>
                                                                    <CST>07</CST>
                                                                </COFINSNT>
                                                            </COFINS>
                                                        </imposto>
                                                    </det>
                                                    <total>
                                                        <ICMSTot>
                                                            <vBC>0.00</vBC>
                                                            <vICMS>0.00</vICMS>
                                                            <vICMSDeson>0.00</vICMSDeson>
                                                            <vBCST>0.00</vBCST>
                                                            <vST>0.00</vST>
                                                            <vProd>689.91</vProd>
                                                            <vFrete>0.00</vFrete>
                                                            <vSeg>0.00</vSeg>
                                                            <vDesc>0.00</vDesc>
                                                            <vII>0.00</vII>
                                                            <vIPI>0.00</vIPI>
                                                            <vPIS>0.00</vPIS>
                                                            <vCOFINS>0.00</vCOFINS>
                                                            <vOutro>0.00</vOutro>
                                                            <vNF>689.91</vNF>
                                                            <vTotTrib>206.97</vTotTrib>
                                                        </ICMSTot>
                                                    </total>
                                                    <transp>
                                                        <modFrete>1</modFrete>
                                                        <transporta>
                                                            <xNome>Cliente Retira</xNome>
                                                            <xEnder>Rua ,</xEnder>
                                                            <xMun>Sao Paulo</xMun>
                                                            <UF>SP</UF>
                                                        </transporta>
                                                        <vol>
                                                            <qVol>1</qVol>
                                                            <marca>S/m</marca>
                                                            <nVol>S/n</nVol>
                                                            <pesoL>0.000</pesoL>
                                                            <pesoB>0.000</pesoB>
                                                        </vol>
                                                    </transp>
                                                    <cobr>
                                                        <fat>
                                                            <nFat>992346</nFat>
                                                            <vOrig>689.91</vOrig>
                                                            <vLiq>689.91</vLiq>
                                                        </fat>
                                                        <dup>
                                                            <nDup>992346</nDup>
                                                            <dVenc>2015-04-24</dVenc>
                                                            <vDup>689.91</vDup>
                                                        </dup>
                                                    </cobr>
                                                    <infAdic>
                                                        <infCpl>""DOCUMENTO EMITIDO POR EMPRESA OPTANTE PELO SIMPLES NACIONAL;NAO
                                                            GERA DIREITO A CREDITO FISCAL DE IPI"";""PERMITE O APROVEITAMENTO DE
                                                            CREDITO DE ICMS NO VALOR DE: R$17,66 CORRESPONDENTE A ALIQUOTA DE
                                                            2.56%"";Vendedor:1 - Guilherme Kavedikado;Valor Aproximado dos Tributos :
                                                            R$ 206,97. Fonte IBPT (Instituto Brasileiro de Planejamento Tributario)</infCpl>
                                                    </infAdic>
                                                </infNFe>
                                                <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
                                                    <SignedInfo>
                                                        <CanonicalizationMethod
                                                            Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
                                                        <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
                                                        <Reference URI=""#NFe35150300822602000124550010009923461099234656"">
                                                            <Transforms>
                                                                <Transform
                                                                    Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
                                                                <Transform
                                                                    Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
                                                            </Transforms>
                                                            <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
                                                            <DigestValue>oWFauN7opm3Q6yKVaHiEBqW3DwU=</DigestValue>
                                                        </Reference>
                                                    </SignedInfo>
                                                    <SignatureValue>KNhzxr9mt2fNcqf4+UIU9XrkzNqw6qg/Mk/uXCXev6YwWf9iF0hLZiRIqKrlUKicCCBzRTxUOiI/
                                                        orc/NtXcAHvX/8LVzlvc/OdiuH+XeqDOgl7KCziu6xN71OW016GQZN6VDOqFwyz3Xp2pPZf56nNs
                                                        5CBiLxPtNvX8CM0oMarUKOl8FFZCTnEwWbGXhbShoQ+2MYS9NnC06TCUjXwVQp6T4UAyLjSFuGbD
                                                        o2XLpzsVU9UQD2qESpSISGwLEVnRaLeeqJI4MRxtwiEBhSvq0R40sI/ejDHkyAx2XT583msAZV32
                                                        i1T+SDM2tIL3zoDQGa4lEm8WxCIKJFluXX7rxg==</SignatureValue>
                                                    <KeyInfo>
                                                        <X509Data>
                                                            <X509Certificate>MIIIajCCBlKgAwIBAgIQTLtMm7tkr6qjM8wZTpUo5jANBgkqhkiG9w0BAQsFADB4MQswCQYDVQQG
                                                                EwJCUjETMBEGA1UEChMKSUNQLUJyYXNpbDE2MDQGA1UECxMtU2VjcmV0YXJpYSBkYSBSZWNlaXRh
                                                                IEZlZGVyYWwgZG8gQnJhc2lsIC0gUkZCMRwwGgYDVQQDExNBQyBDZXJ0aXNpZ24gUkZCIEc0MB4X
                                                                DTE0MTAxMzAwMDAwMFoXDTE1MTAxMjIzNTk1OVowggEMMQswCQYDVQQGEwJCUjETMBEGA1UEChQK
                                                                SUNQLUJyYXNpbDELMAkGA1UECBMCU1AxEjAQBgNVBAcUCVNBTyBQQVVMTzE2MDQGA1UECxQtU2Vj
                                                                cmV0YXJpYSBkYSBSZWNlaXRhIEZlZGVyYWwgZG8gQnJhc2lsIC0gUkZCMRYwFAYDVQQLFA1SRkIg
                                                                ZS1DTlBKIEExMTgwNgYDVQQLFC9BdXRlbnRpY2FkbyBwb3IgQ2VydGlzaWduIENlcnRpZmljYWRv
                                                                cmEgRGlnaXRhbDE9MDsGA1UEAxM0UExPVEFHIFNJU1RFTUFTIEUgU1VQUklNRU5UT1MgTFREQSBN
                                                                RTowMDgyMjYwMjAwMDEyNDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAI0Y640hajWB
                                                                yU0S/7MH47RnCE9dq9Kti72iKBkNwOwZQbysO3InQQZkkZCUn5rGEKKw9R7ddTleZFy3aOR3nGpZ
                                                                qulRP3AkjSWnHmTs1KxdPZra1Py5X0VekDOCk43O1vhsCrml7eiCFzivg5vFwUyAT3u5t8k6Muh9
                                                                6/QymvkQzxhGyIvtB9Qe1256q1oB9HOPSlPijciXrf6d4SdBQouT77W6A1SyOjZ+T/XZhjNXx5HD
                                                                MFyDCEJSM/Zp4k2h+mV7MfVKDKZ2J290YWn9XCI6giLeeNNRS6TK5yrQCZYv0/GiKE3I2nMreEFJ
                                                                qrUpuLpiURJIoqbri59N/AXcxJ0CAwEAAaOCA1gwggNUMIG9BgNVHREEgbUwgbKgPQYFYEwBAwSg
                                                                NAQyMTYxMjE5NjAzNTQ5OTU4MzQwNDAwMDAwMDAwMDAwMDAwMDAwMDU2MjkzNDIzU1NQU1CgJgYF
                                                                YEwBAwKgHQQbTU9OSUNBIE1BUklBIE1VTklaIENBTEFCUklBoBkGBWBMAQMDoBAEDjAwODIyNjAy
                                                                MDAwMTI0oBcGBWBMAQMHoA4EDDAwMDAwMDAwMDAwMIEVc3Vwb3J0ZUBwbG90YWcuY29tLmJyMAkG
                                                                A1UdEwQCMAAwHwYDVR0jBBgwFoAULpHq1m3lslmC3DiFKXY0FlY80D4wDgYDVR0PAQH/BAQDAgXg
                                                                MH8GA1UdIAR4MHYwdAYGYEwBAgEMMGowaAYIKwYBBQUHAgEWXGh0dHA6Ly9pY3AtYnJhc2lsLmNl
                                                                cnRpc2lnbi5jb20uYnIvcmVwb3NpdG9yaW8vZHBjL0FDX0NlcnRpc2lnbl9SRkIvRFBDX0FDX0Nl
                                                                cnRpc2lnbl9SRkIucGRmMIIBFgYDVR0fBIIBDTCCAQkwV6BVoFOGUWh0dHA6Ly9pY3AtYnJhc2ls
                                                                LmNlcnRpc2lnbi5jb20uYnIvcmVwb3NpdG9yaW8vbGNyL0FDQ2VydGlzaWduUkZCRzQvTGF0ZXN0
                                                                Q1JMLmNybDBWoFSgUoZQaHR0cDovL2ljcC1icmFzaWwub3V0cmFsY3IuY29tLmJyL3JlcG9zaXRv
                                                                cmlvL2xjci9BQ0NlcnRpc2lnblJGQkc0L0xhdGVzdENSTC5jcmwwVqBUoFKGUGh0dHA6Ly9yZXBv
                                                                c2l0b3Jpby5pY3BicmFzaWwuZ292LmJyL2xjci9DZXJ0aXNpZ24vQUNDZXJ0aXNpZ25SRkJHNC9M
                                                                YXRlc3RDUkwuY3JsMB0GA1UdJQQWMBQGCCsGAQUFBwMCBggrBgEFBQcDBDCBmwYIKwYBBQUHAQEE
                                                                gY4wgYswXwYIKwYBBQUHMAKGU2h0dHA6Ly9pY3AtYnJhc2lsLmNlcnRpc2lnbi5jb20uYnIvcmVw
                                                                b3NpdG9yaW8vY2VydGlmaWNhZG9zL0FDX0NlcnRpc2lnbl9SRkJfRzQucDdjMCgGCCsGAQUFBzAB
                                                                hhxodHRwOi8vb2NzcC5jZXJ0aXNpZ24uY29tLmJyMA0GCSqGSIb3DQEBCwUAA4ICAQBKs2v9oWD9
                                                                7L3/P3v6Xvfng4Ul1H53BuUPdrQac1lkS9B0Id7NeSrgXFw+Wm6+fanyUsXYeYGsAQ3dw6hIEKS1
                                                                vHm5/8UtL5qaQiuGISY2MxfpUy0gA4qkPB05+eTBr6VUpejpqBORAQTjO6j6NI+HpRsCyTUpG9tJ
                                                                JStGw63QZpMLJCHsh+lKPrl8ESt9FElbsLo8XYqYvClA53gZj3exLKzRgw0ayAW5DYrIOprB0r58
                                                                qLRwLpRdtG4LIQU0JSiFEF2snJ2wGAX1bFuvjmv7QmvTfbeRKH4ttkkU7Fk1im9cN8AxLOg61tZ7
                                                                jR+aTeFXjQ2Bbw9bEzRHGVq3VZOI6007Z7pwOZ4eqBO0I/LT+BHZ2SnFJ8UKOI1xgL5EMapIZLbJ
                                                                +lr3bJcjl0WoPlxZs8TvutjG9Fbv08ZpgPo35IRx9K1aDJ514sDTqHwQgXYI279o7i+JJylH3rDv
                                                                7ahVNgJgkfS/j5b0P1ggwQnPtbSDLPt3LX0A+wa9zrTxz5v0/ALddjEFoBkyp+SN6H605yenmy0x
                                                                Cj7bxTnL+am8nrxufOQXdpHFRGuBhhe0qlRM+EVyGZbl29kN2zm4OHZCA5KAnMcChDZrY3QoYlLK
                                                                k3vVkmzq0AGmoO4CxOr33CBFzLbtDHFAoCotvE+x58E7G3CX3J+t1U5dz8PBBsYNkg==</X509Certificate>
                                                        </X509Data>
                                                    </KeyInfo>
                                                </Signature>
                                            </NFe>
                                        </nfe:nfeDadosMsg>
                                    </soap:Body>
                                </soap:Envelope>
                            </nfe:nfeDadosMsg>
                        </soap:Body>
                    </soap:Envelope>
                "
            );

            return soapEnvelopeDocument;
        }

        private void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }
    }
}