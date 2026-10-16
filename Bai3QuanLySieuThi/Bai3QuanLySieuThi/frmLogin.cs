namespace Bai3QuanLySieuThi;

// Form đăng nhập đơn giản để có "tên người dùng đang đăng nhập" hiển thị trên StatusStrip
public class frmLogin : Form
{
    // Tài khoản demo (bài tập không yêu cầu CSDL)
    private static readonly Dictionary<string, string> Accounts = new()
    {
        ["admin"] = "123",
        ["nhanvien"] = "123"
    };

    private readonly TextBox txtUser = new();
    private readonly TextBox txtPassword = new();
    private readonly Button btnLogin = new();
    private readonly Button btnCancel = new();

    public string UserName { get; private set; } = "";

    public frmLogin()
    {
        Text = "Đăng nhập hệ thống";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(340, 170);

        Controls.Add(new Label { Text = "Tên đăng nhập:", Left = 20, Top = 28, AutoSize = true });
        Controls.Add(new Label { Text = "Mật khẩu:", Left = 20, Top = 68, AutoSize = true });

        txtUser.SetBounds(125, 24, 190, 27);
        txtPassword.SetBounds(125, 64, 190, 27);
        txtPassword.UseSystemPasswordChar = true;
        txtUser.Text = "admin";   // điền sẵn cho tiện test

        btnLogin.Text = "Đăng nhập";
        btnLogin.SetBounds(125, 115, 95, 32);
        btnLogin.Click += (s, e) => DoLogin();

        btnCancel.Text = "Hủy";
        btnCancel.SetBounds(225, 115, 90, 32);
        btnCancel.DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[] { txtUser, txtPassword, btnLogin, btnCancel });

        AcceptButton = btnLogin;   // Enter = Đăng nhập
        CancelButton = btnCancel;  // Esc = Hủy
    }

    private void DoLogin()
    {
        string user = txtUser.Text.Trim();

        if (Accounts.TryGetValue(user, out var pass) && pass == txtPassword.Text)
        {
            UserName = user;
            DialogResult = DialogResult.OK;
            return;
        }

        MessageBox.Show(this, "Sai tên đăng nhập hoặc mật khẩu.\n(Demo: admin / 123)",
            "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        txtPassword.Clear();
        txtPassword.Focus();
    }
}
