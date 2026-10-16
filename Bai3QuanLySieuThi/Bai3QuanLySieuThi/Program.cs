namespace Bai3QuanLySieuThi;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Vòng lặp: Đăng nhập -> Form cha MDI -> (Đăng xuất thì quay lại Đăng nhập)
        while (true)
        {
            string userName;
            using (var login = new frmLogin())
            {
                if (login.ShowDialog() != DialogResult.OK) break;   // bấm Hủy/đóng -> thoát app
                userName = login.UserName;
            }

            var main = new frmMain(userName);
            Application.Run(main);

            if (!main.LoggedOut) break;   // thoát bình thường (không phải đăng xuất)
        }
    }
}
