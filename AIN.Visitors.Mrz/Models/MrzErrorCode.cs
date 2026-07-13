namespace AIN.Visitors.Mrz.Models
{
    public enum MrzErrorCode
    {
        None = 0,               // لا يوجد خطأ، الوثيقة سليمة
        InvalidLength = 101,    // طول السطر غير مطابق للمواصفات
        InvalidFormat = 102,    // عدد الأسطر غير صحيح
        InvalidCharacter = 201, // تحتوي على رموز غير مسموحة (حروف صغيرة، مسافات)
        InvalidDate = 202,      // تاريخ مستحيل (مثل شهر 13)
        DocumentExpired = 203,  // الوثيقة منتهية الصلاحية
        CheckDigitError = 301,  // فشل في مطابقة أرقام التحقق الرياضية
        UnknownError = 999
    }
}