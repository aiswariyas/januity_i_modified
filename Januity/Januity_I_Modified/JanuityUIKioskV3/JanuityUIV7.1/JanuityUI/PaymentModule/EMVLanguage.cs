namespace JanuityPayment
{
    public sealed class EMVLanguage
    {
        public static EMVLanguage LANGUAGE_ENGLISH = new EMVLanguage(new byte[] { 0x65, 0x6E } /* en */, "English");
        public static EMVLanguage LANGUAGE_FRENCH  = new EMVLanguage(new byte[] { 0x66, 0x72 } /* fr */, "Français");
        public static EMVLanguage LANGUAGE_GERMAN  = new EMVLanguage(new byte[] { 0x64, 0x65 } /* de */, "Deutsch");
        public static EMVLanguage LANGUAGE_ITALIAN = new EMVLanguage(new byte[] { 0x66, 0x74 } /* it */, "Italiano");
        public static EMVLanguage LANGUAGE_SPANISH = new EMVLanguage(new byte[] { 0x65, 0x73 } /* es */, "Español");
        public static EMVLanguage LANGUAGE_CHINESE = new EMVLanguage(new byte[] { 0x7A, 0x68 } /* zh */, "中文");

        public static EMVLanguage[] LANGUAGE_LIST = new EMVLanguage[] { EMVLanguage.LANGUAGE_ENGLISH, EMVLanguage.LANGUAGE_FRENCH, LANGUAGE_GERMAN, LANGUAGE_ITALIAN, LANGUAGE_SPANISH, EMVLanguage.LANGUAGE_CHINESE };

        public static EMVLanguage GetLanguage(byte[] code)
        {
            EMVLanguage language = null;

            if (code != null)
            {
                if (code.Length == 2)
                {
                    for (int i = 0; i < LANGUAGE_LIST.Length; i++)
                    {
                        byte[] iCode = LANGUAGE_LIST[i].Code;

                        if ((code[0] == iCode[0]) && (code[1] == iCode[1]))
                        {
                            language = LANGUAGE_LIST[i];
                            break;
                        }
                    }
                }
            }

            return language;
        }

        private byte[] m_code;
        private string m_name;

        public EMVLanguage(byte[] code, string name)
        {
            m_code = (code != null) ? (byte[])code.Clone() : null;
            m_name = name;
        }

        public EMVLanguage(EMVLanguage language)
        {
            m_code = (language.m_code != null) ? (byte[])language.m_code.Clone() : null;
            m_name = language.m_name;
        }

        public byte[] Code
        {
            get { return m_code; }
            set { m_code = (value != null) ? (byte[])value.Clone() : null; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }
    }
}

namespace JanuityUI
{
    class EMVLanguage
    {
    }
}