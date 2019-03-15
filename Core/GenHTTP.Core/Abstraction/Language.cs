using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction
{

    /// <summary>
    /// Languages, following the ISO 639-1 standard.
    /// </summary>
    public enum Language
    {
        /// <summary>
        /// Afar
        /// </summary>
        Afar, // aa
              /// <summary>
              /// Abkhazian
              /// </summary>
        Abkhazian, // ab
                   /// <summary>
                   /// Avestan
                   /// </summary>
        Avestan, // ae
                 /// <summary>
                 /// Afrikaans
                 /// </summary>
        Afrikaans, // af
                   /// <summary>
                   /// Akan
                   /// </summary>
        Akan, // ak
              /// <summary>
              /// Amharic
              /// </summary>
        Amharic, // am,
                 /// <summary>
                 /// Aragonese
                 /// </summary>
        Aragonese, // an
                   /// <summary>
                   /// Arabic
                   /// </summary>
        Arabic, // ar
                /// <summary>
                /// Assamese
                /// </summary>
        Assamese, // as
                  /// <summary>
                  /// Avaric
                  /// </summary>
        Avaric, // av
                /// <summary>
                /// Aymara
                /// </summary>
        Aymara, // ay
                /// <summary>
                /// Azerbaijani
                /// </summary>
        Azerbaijani, // az
                     /// <summary>
                     /// Bashkir
                     /// </summary>
        Bashkir, // ba
                 /// <summary>
                 /// Belarusian
                 /// </summary>
        Belarusian, // be
                    /// <summary>
                    /// Bulgarian
                    /// </summary>
        Bulgarian, // bg
                   /// <summary>
                   /// Bihari
                   /// </summary>
        Bihari, // bh
                /// <summary>
                /// Bislama
                /// </summary>
        Bislama, // bi
                 /// <summary>
                 /// Bambara
                 /// </summary>
        Bambara, // bm
                 /// <summary>
                 /// Bengali
                 /// </summary>
        Bengali, // bn
                 /// <summary>
                 /// Tibetian
                 /// </summary>
        Tibetian, // bo
                  /// <summary>
                  /// Breton
                  /// </summary>
        Breton, // br
                /// <summary>
                /// Bosnian
                /// </summary>
        Bosnian, // bs
                 /// <summary>
                 /// Catalan
                 /// </summary>
        Catalan, // ca
                 /// <summary>
                 /// Valencian
                 /// </summary>
        Valencian, // ca
                   /// <summary>
                   /// Chechen
                   /// </summary>
        Chechen, // ce
                 /// <summary>
                 /// Chamorro
                 /// </summary>
        Chamorro, // ch
                  /// <summary>
                  /// Corsican
                  /// </summary>
        Corsican, // co
                  /// <summary>
                  /// Cree
                  /// </summary>
        Cree, // cr
              /// <summary>
              /// Czech
              /// </summary>
        Czech, // cs
               /// <summary>
               /// ChurchSlavic
               /// </summary>
        ChurchSlavic, // cu
                      /// <summary>
                      /// Chuvash
                      /// </summary>
        Chuvash, // cv
                 /// <summary>
                 /// Welsh
                 /// </summary>
        Welsh, // cy
               /// <summary>
               /// Danish
               /// </summary>
        Danish, // da
                /// <summary>
                /// German
                /// </summary>
        German, // de
                /// <summary>
                /// Divehi
                /// </summary>
        Divehi, // dv
                /// <summary>
                /// Dzongkha
                /// </summary>
        Dzongkha, // dz
                  /// <summary>
                  /// Ewe
                  /// </summary>
        Ewe, // ee
             /// <summary>
             /// Greek
             /// </summary>
        Greek, // el
               /// <summary>
               /// English
               /// </summary>
        English, // en,
                 /// <summary>
                 /// Esperanto
                 /// </summary>
        Esperanto, // eo
                   /// <summary>
                   /// Spanish
                   /// </summary>
        Spanish, // es
                 /// <summary>
                 /// Estonian
                 /// </summary>
        Estonian, // et
                  /// <summary>
                  /// Basque
                  /// </summary>
        Basque, // eu
                /// <summary>
                /// Persian
                /// </summary>
        Persian, // fa
                 /// <summary>
                 /// Fluah
                 /// </summary>
        Fluah, // ff
               /// <summary>
               /// Finnish
               /// </summary>
        Finnish, // fi
                 /// <summary>
                 /// Fijan
                 /// </summary>
        Fijan, // fj
               /// <summary>
               /// Faroese
               /// </summary>
        Faroese, // fo
                 /// <summary>
                 /// French
                 /// </summary>
        French, // fr
                /// <summary>
                /// WesternFrisian
                /// </summary>
        WesternFrisian, // fy
                        /// <summary>
                        /// Irish
                        /// </summary>
        Irish, // ga
               /// <summary>
               /// Scottish
               /// </summary>
        Scottish, // gd
                  /// <summary>
                  /// Galician
                  /// </summary>
        Galician, // gl
                  /// <summary>
                  /// Guaraní
                  /// </summary>
        Guaraní, // gn
                 /// <summary>
                 /// Gujarati
                 /// </summary>
        Gujarati, // gu
                  /// <summary>
                  /// Manx
                  /// </summary>
        Manx, // gv
              /// <summary>
              /// Hausa
              /// </summary>
        Hausa, // ha
               /// <summary>
               /// Hebrew
               /// </summary>
        Hebrew, // he
                /// <summary>
                /// Hindi
                /// </summary>
        Hindi, // hi
               /// <summary>
               /// HiriMotu
               /// </summary>
        HiriMotu, // ho
                  /// <summary>
                  /// Croatian
                  /// </summary>
        Croatian, // hr
                  /// <summary>
                  /// Haitian
                  /// </summary>
        Haitian, // ht
                 /// <summary>
                 /// Hungarian
                 /// </summary>
        Hungarian, // hu
                   /// <summary>
                   /// Armenian
                   /// </summary>
        Armenian, // hy
                  /// <summary>
                  /// Herero
                  /// </summary>
        Herero, // hz
                /// <summary>
                /// Interlingua
                /// </summary>
        Interlingua, // ia
                     /// <summary>
                     /// Indonesian
                     /// </summary>
        Indonesian, // id
                    /// <summary>
                    /// Interlinque
                    /// </summary>
        Interlinque, // ie
                     /// <summary>
                     /// Igbo
                     /// </summary>
        Igbo, // ig
              /// <summary>
              /// SichuanYi
              /// </summary>
        SichuanYi, // ii
                   /// <summary>
                   /// Inupiaq
                   /// </summary>
        Inupiaq, // ik
                 /// <summary>
                 /// Ido
                 /// </summary>
        Ido, // io
             /// <summary>
             /// Icelandic
             /// </summary>
        Icelandic, // is
                   /// <summary>
                   /// Italian
                   /// </summary>
        Italian, // it
                 /// <summary>
                 /// Inukitut
                 /// </summary>
        Inukitut, // iu
                  /// <summary>
                  /// Japanese
                  /// </summary>
        Japanese, // ja
                  /// <summary>
                  /// Javanese
                  /// </summary>
        Javanese, // jv
                  /// <summary>
                  /// Georgian
                  /// </summary>
        Georgian, // ka
                  /// <summary>
                  /// Kongo
                  /// </summary>
        Kongo, // kg
               /// <summary>
               /// Kikuyu
               /// </summary>
        Kikuyu, // ki
                /// <summary>
                /// Kwanyama
                /// </summary>
        Kwanyama, // kj
                  /// <summary>
                  /// Kazakh
                  /// </summary>
        Kazakh, // kk
                /// <summary>
                /// Kalaallisut
                /// </summary>
        Kalaallisut, // kl
                     /// <summary>
                     /// Khmer
                     /// </summary>
        Khmer, // km
               /// <summary>
               /// Kannada
               /// </summary>
        Kannada, // kn
                 /// <summary>
                 /// Korean
                 /// </summary>
        Korean, // ko
                /// <summary>
                /// Kanuri
                /// </summary>
        Kanuri, // kr
                /// <summary>
                /// Kashmiri
                /// </summary>
        Kashmiri, // ks
                  /// <summary>
                  /// Kurdish
                  /// </summary>
        Kurdish, // ku
                 /// <summary>
                 /// Komi
                 /// </summary>
        Komi, // kv
              /// <summary>
              /// Cornish
              /// </summary>
        Cornish, // kw
                 /// <summary>
                 /// Kirghiz
                 /// </summary>
        Kirghiz, // ky
                 /// <summary>
                 /// Latin
                 /// </summary>
        Latin, // la
               /// <summary>
               /// Luxembourgish
               /// </summary>
        Luxembourgish, // lb
                       /// <summary>
                       /// Ganda
                       /// </summary>
        Ganda, // lg
               /// <summary>
               /// Limburgish
               /// </summary>
        Limburgish, // li
                    /// <summary>
                    /// Lingala
                    /// </summary>
        Lingala, // ln
                 /// <summary>
                 /// Lao
                 /// </summary>
        Lao, // lo
             /// <summary>
             /// Lithuanian
             /// </summary>
        Lithuanian, // lt
                    /// <summary>
                    /// LubaKatanga
                    /// </summary>
        LubaKatanga, // lu,
                     /// <summary>
                     /// Latvian
                     /// </summary>
        Latvian, // lv
                 /// <summary>
                 /// Malagasy
                 /// </summary>
        Malagasy, // mg
                  /// <summary>
                  /// Marshallese
                  /// </summary>
        Marshallese, // mh
                     /// <summary>
                     /// Māori
                     /// </summary>
        Māori, // mi
               /// <summary>
               /// Macedonian
               /// </summary>
        Macedonian, // mk
                    /// <summary>
                    /// Malayalam
                    /// </summary>
        Malayalam, // ml
                   /// <summary>
                   /// Mongolian
                   /// </summary>
        Mongolian, // mn
                   /// <summary>
                   /// Marathi
                   /// </summary>
        Marathi, // mr
                 /// <summary>
                 /// Malay
                 /// </summary>
        Malay, // ms
               /// <summary>
               /// Maltese
               /// </summary>
        Maltese, // mt
                 /// <summary>
                 /// Burmese
                 /// </summary>
        Burmese, // my
                 /// <summary>
                 /// Nauru
                 /// </summary>
        Nauru, // na
               /// <summary>
               /// NorwegianBokmål
               /// </summary>
        NorwegianBokmål, // nb
                         /// <summary>
                         /// NorthNdebele
                         /// </summary>
        NorthNdebele, // nd
                      /// <summary>
                      /// Nepali
                      /// </summary>
        Nepali, // ne
                /// <summary>
                /// Ndonga
                /// </summary>
        Ndonga, // ng
                /// <summary>
                /// Dutch
                /// </summary>
        Dutch, // nl
               /// <summary>
               /// Flemish
               /// </summary>
        Flemish, // nl
                 /// <summary>
                 /// Nynorsk
                 /// </summary>
        Nynorsk, // nn
                 /// <summary>
                 /// Norwegian
                 /// </summary>
        Norwegian, // no
                   /// <summary>
                   /// SouthNdebele
                   /// </summary>
        SouthNdebele, // nr
                      /// <summary>
                      /// Navajo
                      /// </summary>
        Navajo, // nv
                /// <summary>
                /// Chichewa
                /// </summary>
        Chichewa, // ny
                  /// <summary>
                  /// Occitan
                  /// </summary>
        Occitan, // oc
                 /// <summary>
                 /// Ojibwa
                 /// </summary>
        Ojibwa, // oj
                /// <summary>
                /// Oromo
                /// </summary>
        Oromo, // om
               /// <summary>
               /// Oriya
               /// </summary>
        Oriya, // or
               /// <summary>
               /// Ossetian
               /// </summary>
        Ossetian, // os
                  /// <summary>
                  /// Panjabi
                  /// </summary>
        Panjabi, // pa
                 /// <summary>
                 /// Pāli
                 /// </summary>
        Pāli, // pi
              /// <summary>
              /// Polish
              /// </summary>
        Polish, // pl
                /// <summary>
                /// Pashto
                /// </summary>
        Pashto, // ps
                /// <summary>
                /// Portugese
                /// </summary>
        Portugese, // pt
                   /// <summary>
                   /// Quechua
                   /// </summary>
        Quechua, // qu
                 /// <summary>
                 /// Romansh
                 /// </summary>
        Romansh, // rm
                 /// <summary>
                 /// Rundi
                 /// </summary>
        Rundi, // rn
               /// <summary>
               /// Romania
               /// </summary>
        Romania, // ro
                 /// <summary>
                 /// Moldavian
                 /// </summary>
        Moldavian, // ro
                   /// <summary>
                   /// Russian
                   /// </summary>
        Russian, // ru
                 /// <summary>
                 /// Kinyarwanda
                 /// </summary>
        Kinyarwanda, // rw
                     /// <summary>
                     /// Sanskrit
                     /// </summary>
        Sanskrit, // sa
                  /// <summary>
                  /// Sardian
                  /// </summary>
        Sardian, // sc
                 /// <summary>
                 /// Sindhi
                 /// </summary>
        Sindhi, // sd
                /// <summary>
                /// NorthernSami
                /// </summary>
        NorthernSami, // se
                      /// <summary>
                      /// Sango
                      /// </summary>
        Sango, // sg
               /// <summary>
               /// Sinhala
               /// </summary>
        Sinhala, // si
                 /// <summary>
                 /// Slovak
                 /// </summary>
        Slovak, // sk
                /// <summary>
                /// Slovene
                /// </summary>
        Slovene, // sl
                 /// <summary>
                 /// Samoan
                 /// </summary>
        Samoan, // sm
                /// <summary>
                /// Shona
                /// </summary>
        Shona, // sn
               /// <summary>
               /// Somali
               /// </summary>
        Somali, // so
                /// <summary>
                /// Albanian
                /// </summary>
        Albanian, // sq
                  /// <summary>
                  /// Serbian
                  /// </summary>
        Serbian, // sr
                 /// <summary>
                 /// Swati
                 /// </summary>
        Swati, // ss
               /// <summary>
               /// SouthernSotho
               /// </summary>
        SouthernSotho, // st
                       /// <summary>
                       /// Sudanese
                       /// </summary>
        Sudanese, // su
                  /// <summary>
                  /// Swedish
                  /// </summary>
        Swedish, // sv
                 /// <summary>
                 /// Swahili
                 /// </summary>
        Swahili, // sw
                 /// <summary>
                 /// Tamil
                 /// </summary>
        Tamil, // ta
               /// <summary>
               /// Telugu
               /// </summary>
        Telugu, // te
                /// <summary>
                /// Tajik
                /// </summary>
        Tajik, // tg
               /// <summary>
               /// Thai
               /// </summary>
        Thai, // th
              /// <summary>
              /// Tigrinya
              /// </summary>
        Tigrinya, // ti
                  /// <summary>
                  /// Turkmen
                  /// </summary>
        Turkmen, // tk
                 /// <summary>
                 /// Tagalog
                 /// </summary>
        Tagalog, // tl
                 /// <summary>
                 /// Tswana
                 /// </summary>
        Tswana, // tn
                /// <summary>
                /// Tonga
                /// </summary>
        Tonga, // to
               /// <summary>
               /// Turkish
               /// </summary>
        Turkish, // tr
                 /// <summary>
                 /// Tsonga
                 /// </summary>
        Tsonga, // ts
                /// <summary>
                /// Tatar
                /// </summary>
        Tatar, // tt
               /// <summary>
               /// Twi
               /// </summary>
        Twi, // tw
             /// <summary>
             /// Tahititan
             /// </summary>
        Tahititan, // ty
                   /// <summary>
                   /// Uighur
                   /// </summary>
        Uighur, // ug
                /// <summary>
                /// Ukranian
                /// </summary>
        Ukranian, // uk
                  /// <summary>
                  /// Urdu
                  /// </summary>
        Urdu, // ur
              /// <summary>
              /// Uzbek
              /// </summary>
        Uzbek, // uz
               /// <summary>
               /// Venda
               /// </summary>
        Venda, // ve
               /// <summary>
               /// Vietnamese
               /// </summary>
        Vietnamese, // vi
                    /// <summary>
                    /// Volapük
                    /// </summary>
        Volapük, // vo
                 /// <summary>
                 /// Walloon
                 /// </summary>
        Walloon, // wa
                 /// <summary>
                 /// Wolof
                 /// </summary>
        Wolof, // wo
               /// <summary>
               /// Xhosa
               /// </summary>
        Xhosa, // xh
               /// <summary>
               /// Yiddish
               /// </summary>
        Yiddish, // yi
                 /// <summary>
                 /// Yoruba
                 /// </summary>
        Yoruba, // yo
                /// <summary>
                /// Zhuang
                /// </summary>
        Zhuang, // za
                /// <summary>
                /// Chinese
                /// </summary>
        Chinese, // zh
                 /// <summary>
                 /// Zuku
                 /// </summary>
        Zuku, // zu
              /// <summary>
              /// No language specified
              /// </summary>
        Unspecified
    }

}
