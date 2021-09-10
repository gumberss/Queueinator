using System;

namespace Queueinator.Forms.Extensions
{
    public static class StringExtension
    {
        public static String Default(this String text, String defaultText)
        {
            return String.IsNullOrEmpty(text)
                ? defaultText
                : text;
        }
    }
}
