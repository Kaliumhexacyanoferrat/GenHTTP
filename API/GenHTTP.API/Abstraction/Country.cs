using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction
{

    /// <summary>
    /// Countries, following the ISO 3166 standard.
    /// </summary>
    public enum Country
    {
        /// <summary>
        /// Afghanistan
        /// </summary>
        Afghanistan, // AF
                     /// <summary>
                     /// Åland Islands
                     /// </summary>
        ÅlandIslands, // AX
                      /// <summary>
                      /// Albania
                      /// </summary>
        Albania, // AL
                 /// <summary>
                 /// Algeria
                 /// </summary>
        Algeria, // DZ
                 /// <summary>
                 /// American Samoa
                 /// </summary>
        AmericanSamoa, // AS
                       /// <summary>
                       /// Andorra
                       /// </summary>
        Andorra, // AD
                 /// <summary>
                 /// Angola
                 /// </summary>
        Angola, // AO
                /// <summary>
                /// Anguilla
                /// </summary>
        Anguilla, // AI
                  /// <summary>
                  /// Antarctica
                  /// </summary>
        Antarctica, // AQ
                    /// <summary>
                    /// Antigua and Barbuda
                    /// </summary>
        AntiguaAndBarbuda, // AG
                           /// <summary>
                           /// Argentinia
                           /// </summary>
        Argentinia, // AR
                    /// <summary>
                    /// Armenia
                    /// </summary>
        Armenia, // AM
                 /// <summary>
                 /// Aruba
                 /// </summary>
        Aruba, // AW
               /// <summary>
               /// Australia
               /// </summary>
        Australia, // AU
                   /// <summary>
                   /// Austria
                   /// </summary>
        Austria, // AT
                 /// <summary>
                 /// Azerbaijan
                 /// </summary>
        Azerbaijan, // AZ	  
                    /// <summary>
                    /// Bahamas
                    /// </summary>
        Bahamas, // BS
                 /// <summary>
                 /// Bahrain
                 /// </summary>
        Bahrain, // BH
                 /// <summary>
                 /// Bangladesh
                 /// </summary>
        Bangladesh, // BD
                    /// <summary>
                    /// Barbados
                    /// </summary>
        Barbados, // BB
                  /// <summary>
                  /// Belarus
                  /// </summary>
        Belarus, // BY
                 /// <summary>
                 /// Belgium
                 /// </summary>
        Belgium, // BE
                 /// <summary>
                 /// Belize
                 /// </summary>
        Belize, // BZ
                /// <summary>
                /// Benin
                /// </summary>
        Benin, // BJ
               /// <summary>
               /// Bermuda
               /// </summary>
        Bermuda, // BM
                 /// <summary>
                 /// Bhutan
                 /// </summary>
        Bhutan, // BT
                /// <summary>
                /// Bolivia
                /// </summary>
        Bolivia, // BO
                 /// <summary>
                 /// Bosnia and Herzegovina
                 /// </summary>
        BosniaAndHerzegovina, // BA
                              /// <summary>
                              /// Botswana
                              /// </summary>
        Botswana, // BW
                  /// <summary>
                  /// Bouvet Island
                  /// </summary>
        BouvetIsland, // BV
                      /// <summary>
                      /// Brazil
                      /// </summary>
        Brazil, // BR
                /// <summary>
                /// British Indian Ocean Territory
                /// </summary>
        BritishIndianOceanTerritory, // IO
                                     /// <summary>
                                     /// Brunei Darussalam
                                     /// </summary>
        BruneiDarussalam, // BN
                          /// <summary>
                          /// Bulgaria
                          /// </summary>
        Bulgaria, // BG
                  /// <summary>
                  /// Burkina Faso
                  /// </summary>
        BurkinaFaso, // BF
                     /// <summary>
                     /// Burundi
                     /// </summary>
        Burundi, // BI	  
                 /// <summary>
                 /// Cambodia
                 /// </summary>
        Cambodia, // KH
                  /// <summary>
                  /// Cameroon
                  /// </summary>
        Cameroon, // CM
                  /// <summary>
                  /// Canada
                  /// </summary>
        Canada, // CA
                /// <summary>
                /// Cape Verde
                /// </summary>
        CapeVerde, // CV
                   /// <summary>
                   /// Cayman Islands
                   /// </summary>
        CaymanIslands, // KY
                       /// <summary>
                       /// Central African Republic
                       /// </summary>
        CentralAfricanRepublic, // CF
                                /// <summary>
                                /// Chad
                                /// </summary>
        Chad, // TD
              /// <summary>
              /// Chile
              /// </summary>
        Chile, // CL
               /// <summary>
               /// China
               /// </summary>
        China, // CN
               /// <summary>
               /// Christmas Island
               /// </summary>
        ChristmasIsland, // CX
                         /// <summary>
                         /// Cocos Keeling Islands
                         /// </summary>
        CocosKeelingIslands, // CC
                             /// <summary>
                             /// Colombia
                             /// </summary>
        Colombia, // CO
                  /// <summary>
                  /// Comboros
                  /// </summary>
        Comboros, // KM
                  /// <summary>
                  /// Congo
                  /// </summary>
        Congo, // CG
               /// <summary>
               /// Democratic Republic of the Congo
               /// </summary>
        DemocraticRepublicOfTheCongo, // CD
                                      /// <summary>
                                      /// Cook Islands
                                      /// </summary>
        CookIslands, // CK
                     /// <summary>
                     /// Costa Rica
                     /// </summary>
        CostaRica, // CR
                   /// <summary>
                   /// Côte d'Ivoire
                   /// </summary>
        CôteIvoire, // CI
                    /// <summary>
                    /// Croatia
                    /// </summary>
        Croatia, // HR
                 /// <summary>
                 /// Cuba
                 /// </summary>
        Cuba, // CU
              /// <summary>
              /// Cyprus
              /// </summary>
        Cyprus, // CY
                /// <summary>
                /// Czech Republic
                /// </summary>
        CzechRepublic, // CZ	
                       /// <summary>
                       /// Denmark
                       /// </summary>
        Denmark, // DK
                 /// <summary>
                 /// Djibouti
                 /// </summary>
        Djibouti, // DJ
                  /// <summary>
                  /// Dominica
                  /// </summary>
        Dominica, // DM
                  /// <summary>
                  /// Dominican Republic
                  /// </summary>
        DominicanRepublic, // DO
                           /// <summary>
                           /// Ecuador
                           /// </summary>
        Ecuador, // EC
                 /// <summary>
                 /// Egypt
                 /// </summary>
        Egypt, // EG
               /// <summary>
               /// El Salvador
               /// </summary>
        ElSalvador, // SV
                    /// <summary>
                    /// Equatorial Guinea
                    /// </summary>
        EquatorialGuinea, // GQ
                          /// <summary>
                          /// Eritrea
                          /// </summary>
        Eritrea, // ER
                 /// <summary>
                 /// Estonia
                 /// </summary>
        Estonia, // EE
                 /// <summary>
                 /// Ethiopia
                 /// </summary>
        Ethiopia, // ET
                  /// <summary>
                  /// Falkland Islands
                  /// </summary>
        FalklandIslands, // FK
                         /// <summary>
                         /// Faroe Islands
                         /// </summary>
        FaroeIslands, // FO
                      /// <summary>
                      /// Fiji
                      /// </summary>
        Fiji, // FJ
              /// <summary>
              /// Finland
              /// </summary>
        Finland, // FI
                 /// <summary>
                 /// France
                 /// </summary>
        France, // FR
                /// <summary>
                /// French Guiana
                /// </summary>
        FrenchGuiana, // GF
                      /// <summary>
                      /// French Polynesia
                      /// </summary>
        FrenchPolynesia, // PF
                         /// <summary>
                         /// French Southern Territories
                         /// </summary>
        FrenchSouthernTerritories, // TF
                                   /// <summary>
                                   /// Gabon
                                   /// </summary>
        Gabon, // GA
               /// <summary>
               /// Gambia
               /// </summary>
        Gambia, // GM
                /// <summary>
                /// Georgia
                /// </summary>
        Georgia, // GE
                 /// <summary>
                 /// Germany
                 /// </summary>
        Germany, // DE
                 /// <summary>
                 /// Ghana
                 /// </summary>
        Ghana, // GH
               /// <summary>
               /// Gibraltar
               /// </summary>
        Gibraltar, // GI
                   /// <summary>
                   /// Greece
                   /// </summary>
        Greece, // GR
                /// <summary>
                /// Greenland
                /// </summary>
        Greenland, // GL
                   /// <summary>
                   /// Grenada
                   /// </summary>
        Grenada, // GD
                 /// <summary>
                 /// Guadeloupe
                 /// </summary>
        Guadeloupe, // GB
                    /// <summary>
                    /// Guam
                    /// </summary>
        Guam, // GU
              /// <summary>
              /// Guatemala
              /// </summary>
        Guatemala, // GT
                   /// <summary>
                   /// Guernsey
                   /// </summary>
        Guernsey, // GG
                  /// <summary>
                  /// Guinea
                  /// </summary>
        Guinea, // GN
                /// <summary>
                /// GuineaBissau
                /// </summary>
        GuineaBissau, // GW
                      /// <summary>
                      /// Guyana
                      /// </summary>
        Guyana, // GY
                /// <summary>
                /// Haiti
                /// </summary>
        Haiti, // HT
               /// <summary>
               /// Heard Island And McDonald Islands
               /// </summary>
        HeardIslandAndMcDonaldIslands, // HM
                                       /// <summary>
                                       /// Holy See (Vatican City)
                                       /// </summary>
        HolySee, // VA
                 /// <summary>
                 /// Honduras
                 /// </summary>
        Honduras, // HN
                  /// <summary>
                  /// HongKong
                  /// </summary>
        HongKong, // HK
                  /// <summary>
                  /// Hungary
                  /// </summary>
        Hungary, // HU
                 /// <summary>
                 /// Iceland
                 /// </summary>
        Iceland, // IS
                 /// <summary>
                 /// India
                 /// </summary>
        India, // IN
               /// <summary>
               /// Indonesia
               /// </summary>
        Indonesia, // ID
                   /// <summary>
                   /// Iran
                   /// </summary>
        Iran, // IR
              /// <summary>
              /// Iraq
              /// </summary>
        Iraq, // RQ
              /// <summary>
              /// Ireland
              /// </summary>
        Ireland, // IE
                 /// <summary>
                 /// Isle of Man
                 /// </summary>
        IsleOfMan, // IM
                   /// <summary>
                   /// Israel
                   /// </summary>
        Israel, // IL
                /// <summary>
                /// Italy
                /// </summary>
        Italy, // IT
               /// <summary>
               /// Jamaica
               /// </summary>
        Jamaica, // JM
                 /// <summary>
                 /// Japan
                 /// </summary>
        Japan, // JP
               /// <summary>
               /// Jersey
               /// </summary>
        Jersey, // JE
                /// <summary>
                /// Jordan
                /// </summary>
        Jordan, // JO
                /// <summary>
                /// Kazakhstan
                /// </summary>
        Kazakhstan, // KZ
                    /// <summary>
                    /// Kenya
                    /// </summary>
        Kenya, // KE
               /// <summary>
               /// Kiribati
               /// </summary>
        Kiribati, // KI
                  /// <summary>
                  /// Democratic People's Republic of Korea
                  /// </summary>
        DemocraticPeoplesRepublicOfKorea, // KP
                                          /// <summary>
                                          /// Republic of Korea
                                          /// </summary>
        RepublicOfKorea, // KR
                         /// <summary>
                         /// Kuwait
                         /// </summary>
        Kuwait, // KW
                /// <summary>
                /// Kyrgyzstan
                /// </summary>
        Kyrgyzstan, // KG
                    /// <summary>
                    /// Lao
                    /// </summary>
        Lao, // LA
             /// <summary>
             /// Latvia
             /// </summary>
        Latvia, // LV
                /// <summary>
                /// Lebanon
                /// </summary>
        Lebanon, // LB
                 /// <summary>
                 /// Lesotho
                 /// </summary>
        Lesotho, // LS
                 /// <summary>
                 /// Libyan Arab Jamahirya
                 /// </summary>
        LibyanArabJamahirya, // LY
                             /// <summary>
                             /// Liechtenstein
                             /// </summary>
        Liechtenstein, // LI
                       /// <summary>
                       /// Lithuania
                       /// </summary>
        Lithuania, // LT
                   /// <summary>
                   /// Luxembourg
                   /// </summary>
        Luxembourg, // LU
                    /// <summary>
                    /// Macao
                    /// </summary>
        Macao, // MO
               /// <summary>
               /// Macedonia
               /// </summary>
        Macedonia, // MK
                   /// <summary>
                   /// Madagascar
                   /// </summary>
        Madagascar, // MG
                    /// <summary>
                    /// Malawi
                    /// </summary>
        Malawi, // MW
                /// <summary>
                /// Malaysia
                /// </summary>
        Malaysia, // MY
                  /// <summary>
                  /// Maldives
                  /// </summary>
        Maldives, // MV
                  /// <summary>
                  /// Mali
                  /// </summary>
        Mali, // ML
              /// <summary>
              /// Malta
              /// </summary>
        Malta, // MT
               /// <summary>
               /// Marshall Islands
               /// </summary>
        MarshallIslands, // MH
                         /// <summary>
                         /// Martinique
                         /// </summary>
        Martinique, // MQ
                    /// <summary>
                    /// Mauritania
                    /// </summary>
        Mauritania, // MR
                    /// <summary>
                    /// Maurutius
                    /// </summary>
        Maurutius, // MU
                   /// <summary>
                   /// Mayotte
                   /// </summary>
        Mayotte, // YT
                 /// <summary>
                 /// Mexico
                 /// </summary>
        Mexico, // MX
                /// <summary>
                /// Micronesia
                /// </summary>
        Micronesia, // FM
                    /// <summary>
                    /// Moldova
                    /// </summary>
        Moldova, // MD
                 /// <summary>
                 /// Monaco
                 /// </summary>
        Monaco, // MC
                /// <summary>
                /// Mongolia
                /// </summary>
        Mongolia, // MN
                  /// <summary>
                  /// Montenegro
                  /// </summary>
        Montenegro, // ME
                    /// <summary>
                    /// Montserrat
                    /// </summary>
        Montserrat, // MS
                    /// <summary>
                    /// Morocco
                    /// </summary>
        Morocco, // MA
                 /// <summary>
                 /// Mozambique
                 /// </summary>
        Mozambique, // MZ
                    /// <summary>
                    /// Myanmar
                    /// </summary>
        Myanmar, // MM
                 /// <summary>
                 /// Namibia
                 /// </summary>
        Namibia, // NA
                 /// <summary>
                 /// Nauru
                 /// </summary>
        Nauru, // NR
               /// <summary>
               /// Nepal
               /// </summary>
        Nepal, // NP
               /// <summary>
               /// Netherlands
               /// </summary>
        Netherlands, // NL
                     /// <summary>
                     /// Netherlands Antilles
                     /// </summary>
        NetherlandsAntilles, // AN
                             /// <summary>
                             /// New Caledonia
                             /// </summary>
        NewCaledonia, // NC
                      /// <summary>
                      /// New Zealand
                      /// </summary>
        NewZealand, // NZ
                    /// <summary>
                    /// Nicaragua
                    /// </summary>
        Nicaragua, // NI
                   /// <summary>
                   /// Niger
                   /// </summary>
        Niger, // NE
               /// <summary>
               /// Nigeria
               /// </summary>
        Nigeria, // NG
                 /// <summary>
                 /// Niue
                 /// </summary>
        Niue, // NU
              /// <summary>
              /// Norfolk Island
              /// </summary>
        NorfolkIsland, // NF
                       /// <summary>
                       /// Northern Mariana Islands
                       /// </summary>
        NorthernMarianaIslands, // MP
                                /// <summary>
                                /// Norway
                                /// </summary>
        Norway, // NO
                /// <summary>
                /// Oman
                /// </summary>
        Oman, // OM
              /// <summary>
              /// Pakistan
              /// </summary>
        Pakistan, // PK
                  /// <summary>
                  /// Palau
                  /// </summary>
        Palau, // PW
               /// <summary>
               /// Palestinian Territory (occupied)
               /// </summary>
        PalestinianTerritory,  // PS
                               /// <summary>
                               /// Panama
                               /// </summary>
        Panama, // PA
                /// <summary>
                /// Papua New Guinea
                /// </summary>
        PapuaNewGuinea, // PG
                        /// <summary>
                        /// Paraguay
                        /// </summary>
        Paraguay, // PY
                  /// <summary>
                  /// Peru
                  /// </summary>
        Peru, // PE
              /// <summary>
              /// Philippines
              /// </summary>
        Philippines, // PH
                     /// <summary>
                     /// Pitcairn
                     /// </summary>
        Pitcairn, // PN
                  /// <summary>
                  /// Poland
                  /// </summary>
        Poland, // PL
                /// <summary>
                /// Portugal
                /// </summary>
        Portugal, // PT
                  /// <summary>
                  /// Puerto Rico
                  /// </summary>
        PuertoRico, // PR
                    /// <summary>
                    /// Qatar
                    /// </summary>
        Qatar, // QA
               /// <summary>
               /// Rèunion
               /// </summary>
        Rèunion, // RE
                 /// <summary>
                 /// Romina
                 /// </summary>
        Romina, // RO
                /// <summary>
                /// Russian Federation
                /// </summary>
        RussianFederation, // RU
                           /// <summary>
                           /// Rwanda
                           /// </summary>
        Rwanda, // RW
                /// <summary>
                /// Saint Barthèlemy
                /// </summary>
        SaintBarthèlemy, // BL
                         /// <summary>
                         /// Saint Helena
                         /// </summary>
        SaintHelena, // SH
                     /// <summary>
                     /// Saint Kitts And Nevis
                     /// </summary>
        SaintKittsAndNevis, // KN
                            /// <summary>
                            /// Saint Lucia
                            /// </summary>
        SaintLucia, // LC
                    /// <summary>
                    /// Saint Martin
                    /// </summary>
        SaintMartin, // MF
                     /// <summary>
                     /// Saint Pierre And Miquelon
                     /// </summary>
        SaintPierreAndMiquelon, // PM
                                /// <summary>
                                /// Saint Vincent And The Grenadines
                                /// </summary>
        SaintVincentAndTheGrenadines, // VC
                                      /// <summary>
                                      /// Samoa
                                      /// </summary>
        Samoa, // WS
               /// <summary>
               /// San Marino
               /// </summary>
        SanMarino, // SM
                   /// <summary>
                   /// Sao Tome And Principe
                   /// </summary>
        SaoTomeAndPrincipe, // ST
                            /// <summary>
                            /// Saudi Arabia
                            /// </summary>
        SaudiArabia, // SA
                     /// <summary>
                     /// Senegal
                     /// </summary>
        Senegal, // SN
                 /// <summary>
                 /// Serbia
                 /// </summary>
        Serbia, // RS
                /// <summary>
                /// Seychelles
                /// </summary>
        Seychelles, // SC
                    /// <summary>
                    /// Sierra Leone
                    /// </summary>
        SierraLeone, // SL
                     /// <summary>
                     /// Singapore
                     /// </summary>
        Singapore, // SG
                   /// <summary>
                   /// Slovakia
                   /// </summary>
        Slovakia, // SK
                  /// <summary>
                  /// Slovenia
                  /// </summary>
        Slovenia, // SI
                  /// <summary>
                  /// Solomon Islands
                  /// </summary>
        SolomonIslands, // SB
                        /// <summary>
                        /// Somalia
                        /// </summary>
        Somalia, // SO
                 /// <summary>
                 /// South Africa
                 /// </summary>
        SouthAfrica, // ZA
                     /// <summary>
                     /// South Georgia And The South Sandwich Islands
                     /// </summary>
        SouthGeorgiaAndTheSouthSandwichIslands, // GS
                                                /// <summary>
                                                /// Spain
                                                /// </summary>
        Spain, // ES
               /// <summary>
               /// Sri Lanka
               /// </summary>
        SriLanka, // LK
                  /// <summary>
                  /// Sudan
                  /// </summary>
        Sudan, // SD
               /// <summary>
               /// Suriname
               /// </summary>
        Suriname, // SR
                  /// <summary>
                  /// Svalbard And Jan Mayen
                  /// </summary>
        SvalbardAndJanMayen, // SJ
                             /// <summary>
                             /// Sweden
                             /// </summary>
        Sweden, // SE
                /// <summary>
                /// Switzerland
                /// </summary>
        Switzerland, // CH
                     /// <summary>
                     /// Syrian Arab Republic
                     /// </summary>
        SyrianArabRepublic, // SY
                            /// <summary>
                            /// Taiwan
                            /// </summary>
        Taiwan, // TW
                /// <summary>
                /// Tajikistan
                /// </summary>
        Tajikistan, // TJ
                    /// <summary>
                    /// Tanzania
                    /// </summary>
        Tanzania, // TZ
                  /// <summary>
                  /// Thailand
                  /// </summary>
        Thailand, // TH
                  /// <summary>
                  /// Timor Leste
                  /// </summary>
        TimorLeste, // TL
                    /// <summary>
                    /// Tongo
                    /// </summary>
        Tongo, // TG
               /// <summary>
               /// Trinidad And Tobago
               /// </summary>
        TrinidadAndTobago, // TT
                           /// <summary>
                           /// Tunisia
                           /// </summary>
        Tunisia, // TN
                 /// <summary>
                 /// Turkey
                 /// </summary>
        Turkey, // TR
                /// <summary>
                /// Turkmenistan
                /// </summary>
        Turkmenistan, // TM
                      /// <summary>
                      /// Turks And Caicos Islands
                      /// </summary>
        TurksAndCaicosIslands, // TC
                               /// <summary>
                               /// Tuvalu
                               /// </summary>
        Tuvalu, // TV
                /// <summary>
                /// Uganda
                /// </summary>
        Uganda, // UG
                /// <summary>
                /// Ukraine
                /// </summary>
        Ukraine, // UA
                 /// <summary>
                 /// United Arab Emirates
                 /// </summary>
        UnitedArabEmirates, // AE
                            /// <summary>
                            /// United Kingdom
                            /// </summary>
        UnitedKingdom, // GB
                       /// <summary>
                       /// United States
                       /// </summary>
        UnitedStates, // US
                      /// <summary>
                      /// United States Minor Outlying Islands
                      /// </summary>
        UnitedStatesMinorOutlyingIslands, // UM
                                          /// <summary>
                                          /// Uruguay
                                          /// </summary>
        Uruguay, // UY
                 /// <summary>
                 /// Uzbekistan
                 /// </summary>
        Uzbekistan, // UZ
                    /// <summary>
                    /// Vanuatu
                    /// </summary>
        Vanuatu, // VU
                 /// <summary>
                 /// Venezuela
                 /// </summary>
        Venezuela, // VE
                   /// <summary>
                   /// Viet Nam
                   /// </summary>
        VietNam, // VN
                 /// <summary>
                 /// British Virgin Islands
                 /// </summary>
        VirginIslandsBritish, // VG
                              /// <summary>
                              /// U.S. Virgin Islands
                              /// </summary>
        VirginIslandsUS, // VI
                         /// <summary>
                         /// Wallis And Futuna
                         /// </summary>
        WallisAndFutuna, // WF
                         /// <summary>
                         /// Western Sahara
                         /// </summary>
        WesternSahara, // EH
                       /// <summary>
                       /// Yemen
                       /// </summary>
        Yemen, // YE
               /// <summary>
               /// Zimbabwe
               /// </summary>
        Zimbabwe, // ZW
                  /// <summary>
                  /// No country selected.
                  /// </summary>
        Unspecified
    }

}
