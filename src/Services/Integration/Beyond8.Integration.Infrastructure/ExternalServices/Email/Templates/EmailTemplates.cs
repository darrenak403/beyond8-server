namespace Beyond8.Integration.Infrastructure.ExternalServices.Email.Templates;

public static class EmailTemplates
{
    public static string GetOtpEmailTemplate(string otpCode, string purpose)
    {
        return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Mã xác thực OTP</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;'>Beyond8</h1>
                            <p style='color: #e0e0e0; margin: 10px 0 0 0; font-size: 14px;'>Nền tảng học tập trực tuyến</p>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <h2 style='color: #333333; margin: 0 0 20px 0; font-size: 24px;'>Mã xác thực OTP</h2>
                            <p style='color: #666666; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>
                                Bạn đã yêu cầu mã OTP cho <strong>{purpose}</strong>. Vui lòng sử dụng mã bên dưới để hoàn tất:
                            </p>

                            <!-- OTP Code Box -->
                            <table width='100%' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td align='center' style='padding: 20px; background-color: #f8f9fa; border-radius: 8px; border: 2px dashed #667eea;'>
                                        <div style='font-size: 36px; font-weight: 700; color: #667eea; letter-spacing: 8px; font-family: monospace;'>
                                            {otpCode}
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <p style='color: #999999; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0;'>
                                ⏰ Mã OTP này sẽ hết hạn sau <strong>5 phút</strong>.<br>
                                🔒 Vui lòng không chia sẻ mã này với bất kỳ ai.
                            </p>

                            <p style='color: #999999; font-size: 13px; line-height: 1.6; margin: 30px 0 0 0; padding-top: 20px; border-top: 1px solid #eeeeee;'>
                                Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8f9fa; padding: 30px; text-align: center;'>
                            <p style='color: #999999; font-size: 12px; margin: 0; line-height: 1.6;'>
                                © 2026 Beyond8. All rights reserved.<br>
                                Nền tảng học tập trực tuyến hàng đầu Việt Nam
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static string GetInstructorApprovalEmailTemplate(string instructorName, string profileUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Chúc mừng! Bạn đã trở thành Giảng viên</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); padding: 40px 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;'>🎉 Chúc mừng!</h1>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <h2 style='color: #333333; margin: 0 0 20px 0; font-size: 24px;'>Xin chào {instructorName},</h2>

                            <p style='color: #666666; font-size: 16px; line-height: 1.8; margin: 0 0 20px 0;'>
                                Chúng tôi rất vui mừng thông báo rằng đơn đăng ký trở thành <strong style='color: #11998e;'>Giảng viên</strong>
                                của bạn đã được <strong>phê duyệt thành công</strong>! 🎓
                            </p>

                            <div style='background-color: #f0fdf4; border-left: 4px solid #11998e; padding: 20px; margin: 30px 0; border-radius: 4px;'>
                                <p style='color: #166534; font-size: 15px; margin: 0; line-height: 1.6;'>
                                    ✅ Tài khoản của bạn đã được nâng cấp lên quyền Giảng viên<br>
                                    ✅ Bạn có thể bắt đầu tạo và quản lý khóa học<br>
                                    ✅ Chia sẻ kiến thức của bạn với hàng nghìn học viên
                                </p>
                            </div>

                            <h3 style='color: #333333; margin: 30px 0 15px 0; font-size: 18px;'>Bước tiếp theo:</h3>
                            <ul style='color: #666666; font-size: 15px; line-height: 2; margin: 0 0 30px 0; padding-left: 20px;'>
                                <li>Hoàn thiện hồ sơ giảng viên của bạn</li>
                                <li>Tạo khóa học đầu tiên</li>
                                <li>Thiết lập phương thức thanh toán</li>
                                <li>Tham gia cộng đồng giảng viên Beyond8</li>
                            </ul>

                            <!-- CTA Button -->
                            <table width='100%' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td align='center' style='padding: 20px 0;'>
                                        <a href='{profileUrl}' style='display: inline-block; background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: #ffffff; text-decoration: none; padding: 15px 40px; border-radius: 6px; font-size: 16px; font-weight: 600; box-shadow: 0 4px 6px rgba(17, 153, 142, 0.3);'>
                                            Xem hồ sơ giảng viên
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p style='color: #999999; font-size: 14px; line-height: 1.6; margin: 30px 0 0 0; padding-top: 20px; border-top: 1px solid #eeeeee;'>
                                💡 <strong>Mẹo:</strong> Giảng viên có hồ sơ đầy đủ và khóa học chất lượng sẽ được ưu tiên hiển thị trên trang chủ!
                            </p>

                            <p style='color: #666666; font-size: 15px; line-height: 1.6; margin: 20px 0 0 0;'>
                                Chúc bạn thành công trên hành trình chia sẻ tri thức! 🚀
                            </p>

                            <p style='color: #666666; font-size: 15px; margin: 20px 0 0 0;'>
                                Trân trọng,<br>
                                <strong>Đội ngũ Beyond8</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8f9fa; padding: 30px; text-align: center;'>
                            <p style='color: #999999; font-size: 12px; margin: 0 0 10px 0;'>
                                Cần hỗ trợ? Liên hệ với chúng tôi tại
                                <a href='mailto:support@beyond8.dev' style='color: #11998e; text-decoration: none;'>support@beyond8.dev</a>
                            </p>
                            <p style='color: #999999; font-size: 12px; margin: 0; line-height: 1.6;'>
                                © 2026 Beyond8. All rights reserved.<br>
                                Nền tảng học tập trực tuyến hàng đầu Việt Nam
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static string GetInstructorRejectionEmailTemplate(string instructorName, string reason)
    {
        return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Thông báo về đơn đăng ký Giảng viên</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;'>Beyond8</h1>
                            <p style='color: #e0e0e0; margin: 10px 0 0 0; font-size: 14px;'>Nền tảng học tập trực tuyến</p>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <h2 style='color: #333333; margin: 0 0 20px 0; font-size: 24px;'>Xin chào {instructorName},</h2>

                            <p style='color: #666666; font-size: 16px; line-height: 1.8; margin: 0 0 20px 0;'>
                                Cảm ơn bạn đã quan tâm và đăng ký trở thành Giảng viên tại Beyond8.
                            </p>

                            <p style='color: #666666; font-size: 16px; line-height: 1.8; margin: 0 0 20px 0;'>
                                Sau khi xem xét kỹ lưỡng, chúng tôi rất tiếc phải thông báo rằng đơn đăng ký của bạn
                                <strong style='color: #dc2626;'>chưa được phê duyệt</strong> tại thời điểm này.
                            </p>

                            <div style='background-color: #fef2f2; border-left: 4px solid #dc2626; padding: 20px; margin: 30px 0; border-radius: 4px;'>
                                <h3 style='color: #991b1b; margin: 0 0 10px 0; font-size: 16px;'>Lý do:</h3>
                                <p style='color: #7f1d1d; font-size: 15px; margin: 0; line-height: 1.6;'>
                                    {reason}
                                </p>
                            </div>

                            <h3 style='color: #333333; margin: 30px 0 15px 0; font-size: 18px;'>Bạn có thể làm gì tiếp theo?</h3>
                            <ul style='color: #666666; font-size: 15px; line-height: 2; margin: 0 0 30px 0; padding-left: 20px;'>
                                <li>Xem lại và cập nhật hồ sơ theo yêu cầu</li>
                                <li>Bổ sung thêm chứng chỉ và kinh nghiệm</li>
                                <li>Đăng ký lại sau khi hoàn thiện hồ sơ</li>
                                <li>Liên hệ với chúng tôi nếu cần hỗ trợ thêm</li>
                            </ul>

                            <p style='color: #666666; font-size: 15px; line-height: 1.6; margin: 20px 0;'>
                                Chúng tôi luôn chào đón những giảng viên tài năng và nhiệt huyết.
                                Hy vọng sẽ được hợp tác với bạn trong tương lai!
                            </p>

                            <p style='color: #666666; font-size: 15px; margin: 20px 0 0 0;'>
                                Trân trọng,<br>
                                <strong>Đội ngũ Beyond8</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8f9fa; padding: 30px; text-align: center;'>
                            <p style='color: #999999; font-size: 12px; margin: 0 0 10px 0;'>
                                Cần hỗ trợ? Liên hệ với chúng tôi tại
                                <a href='mailto:support@beyond8.dev' style='color: #667eea; text-decoration: none;'>support@beyond8.dev</a>
                            </p>
                            <p style='color: #999999; font-size: 12px; margin: 0; line-height: 1.6;'>
                                © 2026 Beyond8. All rights reserved.<br>
                                Nền tảng học tập trực tuyến hàng đầu Việt Nam
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    public static string GetInstructorUpdateRequestEmailTemplate(string instructorName, string updateNotes)
    {
        return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Yêu cầu cập nhật hồ sơ Giảng viên</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); padding: 40px 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;'>Beyond8</h1>
                            <p style='color: #fef3c7; margin: 10px 0 0 0; font-size: 14px;'>Nền tảng học tập trực tuyến</p>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <h2 style='color: #333333; margin: 0 0 20px 0; font-size: 24px;'>Xin chào {instructorName},</h2>

                            <p style='color: #666666; font-size: 16px; line-height: 1.8; margin: 0 0 20px 0;'>
                                Cảm ơn bạn đã gửi đơn đăng ký trở thành Giảng viên tại Beyond8.
                            </p>

                            <p style='color: #666666; font-size: 16px; line-height: 1.8; margin: 0 0 20px 0;'>
                                Sau khi xem xét hồ sơ của bạn, chúng tôi cần bạn <strong style='color: #f59e0b;'>cập nhật thêm một số thông tin</strong>
                                để hoàn tất quá trình xét duyệt.
                            </p>

                            <div style='background-color: #fffbeb; border-left: 4px solid #f59e0b; padding: 20px; margin: 30px 0; border-radius: 4px;'>
                                <h3 style='color: #92400e; margin: 0 0 10px 0; font-size: 16px;'>📝 Yêu cầu cập nhật:</h3>
                                <p style='color: #78350f; font-size: 15px; margin: 0; line-height: 1.6; white-space: pre-wrap;'>
{updateNotes}
                                </p>
                            </div>

                            <h3 style='color: #333333; margin: 30px 0 15px 0; font-size: 18px;'>Bước tiếp theo:</h3>
                            <ul style='color: #666666; font-size: 15px; line-height: 2; margin: 0 0 30px 0; padding-left: 20px;'>
                                <li>Đăng nhập vào tài khoản Beyond8</li>
                                <li>Vào phần Hồ sơ Giảng viên</li>
                                <li>Cập nhật thông tin theo yêu cầu bên trên</li>
                                <li>Gửi lại đơn để chúng tôi xem xét</li>
                            </ul>

                            <p style='color: #666666; font-size: 15px; line-height: 1.6; margin: 20px 0;'>
                                Chúng tôi mong muốn được hợp tác cùng bạn và hy vọng sớm nhận được hồ sơ cập nhật từ bạn!
                            </p>

                            <p style='color: #666666; font-size: 15px; margin: 20px 0 0 0;'>
                                Trân trọng,<br>
                                <strong>Đội ngũ Beyond8</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8f9fa; padding: 30px; text-align: center;'>
                            <p style='color: #999999; font-size: 12px; margin: 0 0 10px 0;'>
                                Cần hỗ trợ? Liên hệ với chúng tôi tại
                                <a href='mailto:support@beyond8.dev' style='color: #f59e0b; text-decoration: none;'>support@beyond8.dev</a>
                            </p>
                            <p style='color: #999999; font-size: 12px; margin: 0; line-height: 1.6;'>
                                © 2026 Beyond8. All rights reserved.<br>
                                Nền tảng học tập trực tuyến hàng đầu Việt Nam
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
