using ClosedXML.Excel;
using QuanLyBanHang.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace QuanLyBanHang.Forms
{

    public partial class frmHoaDon_ChiTiet : Form
    {

        QLBHDbContext context = new QLBHDbContext();    // Khởi tạo biến ngữ cảnh CSDL 
        int id;                                         // Lấy mã hóa đơn (dùng cho Sửa và Xóa) 
        BindingList<DanhSachHoaDon_ChiTiet> hoaDonChiTiet = new BindingList<DanhSachHoaDon_ChiTiet>();
        public frmHoaDon_ChiTiet(int maHoaDon = 0)
        {
            InitializeComponent();
            id = maHoaDon;
        }

        public void LayNhanVienVaoComboBox()
        {
            cboNhanVien.DataSource = context.NhanViens.ToList();
            cboNhanVien.ValueMember = "ID";
            cboNhanVien.DisplayMember = "HoVaTen";
        }

        public void LayKhachHangVaoComboBox()
        {
            cboKhachHang.DataSource = context.KhachHangs.ToList();
            cboKhachHang.ValueMember = "ID";
            cboKhachHang.DisplayMember = "HoVaTen";
        }

        public void LaySanPhamVaoComboBox()
        {
            cboSanPham.DataSource = context.SanPhams.ToList();
            cboSanPham.ValueMember = "ID";
            cboSanPham.DisplayMember = "TenSanPham";
        }

        public void BatTatChucNang()
        {
            if (id == 0 && dataGridView.Rows.Count == 0) // Thêm 
            {
                // Xóa trắng các trường 
                cboKhachHang.Text = "";
                cboNhanVien.Text = "";
                cboSanPham.Text = "";
                numSoLuongBan.Value = 1;
                numDonGiaBan.Value = 0;
            }
            // Nút lưu và xóa chỉ sáng khi có sản phẩm 
            btnLuuHoaDon.Enabled = dataGridView.Rows.Count > 0;
            btnXoa.Enabled = dataGridView.Rows.Count > 0;
        }

        private void frmHoaDon_ChiTiet_Load(object sender, EventArgs e)
        {
            LayNhanVienVaoComboBox();
            LayKhachHangVaoComboBox();
            LaySanPhamVaoComboBox();
            dataGridView.AutoGenerateColumns = false;
            if (id != 0) // Đã tồn tại chi tiết 
            {
                var hoaDon = context.HoaDons.Where(r => r.Id == id).SingleOrDefault();
                cboNhanVien.SelectedValue = hoaDon.NhanVienId;
                cboKhachHang.SelectedValue = hoaDon.KhachHangId;
                txtGhiChuHoaDon.Text = hoaDon.GhiChuHoaDon;

                var ct = context.HoaDonChiTiets.Where(r => r.HoaDonId == id).Select(r => new DanhSachHoaDon_ChiTiet
                {
                    ID = r.Id,
                    HoaDonID = r.HoaDonId,
                    SanPhamID = r.SanPhamId,
                    TenSanPham = r.SanPham.TenSanPham,
                    SoLuongBan = r.SoLuongBan,
                    DonGiaBan = r.DonGiaBan,
                    ThanhTien = Convert.ToInt32(r.SoLuongBan * r.DonGiaBan)
                }).ToList();

                hoaDonChiTiet = new BindingList<DanhSachHoaDon_ChiTiet>(ct);
            }

            dataGridView.DataSource = hoaDonChiTiet;
            BatTatChucNang();


        }

        private void btnXacNhanBan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboSanPham.Text))
                MessageBox.Show("Vui lòng chọn sản phẩm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (numSoLuongBan.Value <= 0)
                MessageBox.Show("Số lượng bán phải lớn hơn 0.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (numDonGiaBan.Value <= 0)
                MessageBox.Show("Đơn giá bán sản phẩm phải lớn hơn 0.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            {
                int maSanPham = Convert.ToInt32(cboSanPham.SelectedValue.ToString());
                var chiTiet = hoaDonChiTiet.FirstOrDefault(x => x.SanPhamID == maSanPham);

                // Nếu đã tồn tại sản phẩm thì cập nhật thông tin  
                if (chiTiet != null)
                {
                    chiTiet.SoLuongBan = Convert.ToInt16(numSoLuongBan.Value);
                    chiTiet.DonGiaBan = Convert.ToInt32(numDonGiaBan.Value);
                    chiTiet.ThanhTien = Convert.ToInt32(numSoLuongBan.Value * numDonGiaBan.Value);
                    dataGridView.Refresh();
                }
                else // Nếu chưa có sản phẩm thì thêm vào 
                {
                    // Nếu chưa có sản phẩm nào 
                    DanhSachHoaDon_ChiTiet ct = new DanhSachHoaDon_ChiTiet
                    {
                        ID = 0,
                        HoaDonID = id,
                        SanPhamID = maSanPham,
                        TenSanPham = cboSanPham.Text,
                        SoLuongBan = Convert.ToInt16(numSoLuongBan.Value),
                        DonGiaBan = Convert.ToInt32(numDonGiaBan.Value),
                        ThanhTien = Convert.ToInt32(numSoLuongBan.Value * numDonGiaBan.Value)
                    };
                    hoaDonChiTiet.Add(ct);
                }
                BatTatChucNang();
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            int maSanPham = Convert.ToInt32(dataGridView.CurrentRow.Cells["SanPhamID"].Value.ToString());
            var chiTiet = hoaDonChiTiet.FirstOrDefault(x => x.SanPhamID == maSanPham);
            if (chiTiet != null)
            {
                hoaDonChiTiet.Remove(chiTiet);
            }
            BatTatChucNang();
        }

        private void btnLuuHoaDon_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboNhanVien.Text))
                MessageBox.Show("Vui lòng chọn nhân viên lập hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (string.IsNullOrWhiteSpace(cboKhachHang.Text))
                MessageBox.Show("Vui lòng chọn khách hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                if (id != 0) // Đã tồn tại chi tiết thì chỉ cập nhật 
                {
                    HoaDon hd = context.HoaDons.Find(id);
                    if (hd != null)
                    {
                        hd.NhanVienId = Convert.ToInt32(cboNhanVien.SelectedValue.ToString());
                        hd.KhachHangId = Convert.ToInt32(cboKhachHang.SelectedValue.ToString());
                        hd.GhiChuHoaDon = txtGhiChuHoaDon.Text;
                        context.HoaDons.Update(hd);
                        // Xóa chi tiết cũ 
                        var old = context.HoaDonChiTiets.Where(r => r.HoaDonId == id).ToList();
                        context.HoaDonChiTiets.RemoveRange(old);

                        // Thêm lại chi tiết mới 
                        foreach (var item in hoaDonChiTiet.ToList())
                        {
                            HoaDon_ChiTiet ct = new HoaDon_ChiTiet();
                            ct.HoaDonId = id;
                            ct.SanPhamId = item.SanPhamID;
                            ct.SoLuongBan = item.SoLuongBan;
                            ct.DonGiaBan = item.DonGiaBan;
                            context.HoaDonChiTiets.Add(ct);
                        }

                        context.SaveChanges();
                    }
                }
                else // Thêm mới 
                {
                    HoaDon hd = new HoaDon();
                    hd.NhanVienId = Convert.ToInt32(cboNhanVien.SelectedValue.ToString());
                    hd.KhachHangId = Convert.ToInt32(cboKhachHang.SelectedValue.ToString());
                    hd.NgayLap = DateTime.Now;
                    hd.GhiChuHoaDon = txtGhiChuHoaDon.Text;
                    context.HoaDons.Add(hd);
                    context.SaveChanges();

                    id = hd.Id; // thêm dòng này cho CTHĐ

                    // Thêm chi tiết 
                    foreach (var item in hoaDonChiTiet.ToList())
                    {
                        HoaDon_ChiTiet ct = new HoaDon_ChiTiet();
                        ct.HoaDonId = hd.Id;
                        ct.SanPhamId = item.SanPhamID;
                        ct.SoLuongBan = item.SoLuongBan;
                        ct.DonGiaBan = item.DonGiaBan;
                        context.HoaDonChiTiets.Add(ct);
                    }
                    context.SaveChanges();
                }

                MessageBox.Show("Đã lưu thành công!", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


        }

        private void cboSanPham_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int maSanPham = Convert.ToInt32(cboSanPham.SelectedValue.ToString());
            var sanPham = context.SanPhams.Find(maSanPham);
            numDonGiaBan.Value = sanPham.DonGia;
        }

        private void btnInHoaDon_Click(object sender, EventArgs e)
        {
            if (id == 0)
            {
                MessageBox.Show("Bạn chưa lưu hóa đơn!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var hoaDon = context.HoaDons.Find(id);

            var chiTiet = context.HoaDonChiTiets
                .Where(r => r.HoaDonId == id)
                .ToList();

            if (chiTiet.Count == 0)
            {
                MessageBox.Show("Hóa đơn chưa có sản phẩm!", "Thông báo");
                return;
            }

            string s = "===== HÓA ĐƠN =====\n\n";
            s += "Mã hóa đơn: " + id + "\n";
            s += "Nhân viên: " + cboNhanVien.Text + "\n";
            s += "Khách hàng: " + cboKhachHang.Text + "\n\n";

            decimal tongTien = 0;

            foreach (var ct in chiTiet)
            {
                var sp = context.SanPhams.Find(ct.SanPhamId);

                decimal thanhTien = ct.SoLuongBan * ct.DonGiaBan;

                s += sp.TenSanPham + " | ";
                s += ct.SoLuongBan + " x ";
                s += ct.DonGiaBan + " = ";
                s += thanhTien + "\n";

                tongTien += thanhTien;
            }

            s += "\nTổng tiền: " + tongTien + " VNĐ";

            MessageBox.Show(s, "Hóa đơn");
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show("Bạn có muốn thoát không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnXuat_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Xuất Chi Tiết Hóa Đơn";
            saveFileDialog.Filter = "Excel|*.xls;*.xlsx";
            saveFileDialog.FileName = "ChiTietHoaDon.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DataTable table = new DataTable();

                    table.Columns.Add("HoaDonID", typeof(int));
                    table.Columns.Add("SanPhamID", typeof(int));
                    table.Columns.Add("SoLuong", typeof(int));
                    table.Columns.Add("DonGia", typeof(decimal));

                    var ds = context.HoaDonChiTiets.ToList();

                    foreach (var item in ds)
                    {
                        table.Rows.Add(
                            item.HoaDonId,
                            item.SanPhamId,
                            item.SoLuongBan,
                            item.DonGiaBan
                        );
                    }

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(table, "ChiTietHoaDon");
                        wb.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Xuất Excel thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnNhap_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Nhập Chi Tiết Hóa Đơn từ Excel";
            openFileDialog.Filter = "Excel|*.xls;*.xlsx";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DataTable table = new DataTable();

                    using (XLWorkbook workbook = new XLWorkbook(openFileDialog.FileName))
                    {
                        IXLWorksheet worksheet = workbook.Worksheet(1);
                        bool firstRow = true;
                        string readRange = "";

                        foreach (IXLRow row in worksheet.RowsUsed())
                        {
                            if (firstRow)
                            {
                                readRange = "1:" + row.LastCellUsed().Address.ColumnNumber;

                                foreach (IXLCell cell in row.Cells(readRange))
                                {
                                    table.Columns.Add(cell.Value.ToString());
                                }

                                firstRow = false;
                            }
                            else
                            {
                                table.Rows.Add();
                                int cellIndex = 0;

                                foreach (IXLCell cell in row.Cells(readRange))
                                {
                                    table.Rows[table.Rows.Count - 1][cellIndex] = cell.Value.ToString();
                                    cellIndex++;
                                }
                            }
                        }

                        foreach (DataRow r in table.Rows)
                        {
                            HoaDon_ChiTiet ct = new HoaDon_ChiTiet();

                            ct.HoaDonId = int.Parse(r["HoaDonID"].ToString());
                            ct.SanPhamId = int.Parse(r["SanPhamID"].ToString());
                            ct.SoLuongBan = short.Parse(r["SoLuong"].ToString());
                            ct.DonGiaBan = decimal.Parse(r["DonGia"].ToString());

                            context.HoaDonChiTiets.Add(ct);
                        }

                        context.SaveChanges();

                        MessageBox.Show("Đã nhập " + table.Rows.Count + " dòng.",
                            "Thành công",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        frmHoaDon_ChiTiet_Load(sender, e);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
