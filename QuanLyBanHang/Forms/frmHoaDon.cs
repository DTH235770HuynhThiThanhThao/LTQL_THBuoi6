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
using Excel = Microsoft.Office.Interop.Excel;
using ClosedXML.Excel;

namespace QuanLyBanHang.Forms
{
    public partial class frmHoaDon : Form
    {
        QLBHDbContext context = new QLBHDbContext();
        int id;
        public frmHoaDon()
        {
            InitializeComponent();
        }

        private void frmHoaDon_Load(object sender, EventArgs e)
        {
            dataGridView.AutoGenerateColumns = false;
            List<DanhSachHoaDon> hd = new List<DanhSachHoaDon>();
            hd = context.HoaDons.Select(r => new DanhSachHoaDon
            {

                Id = r.Id,
                NhanVienId = r.NhanVienId,
                HoVaTenNhanVien = r.NhanVien.HoVaTen,
                KhachHangId = r.KhachHangId,
                HoVaTenKhachHang = r.KhachHang.HoVaTen,
                NgayLap = r.NgayLap,
                GhiChuHoaDon = r.GhiChuHoaDon,
                TongTienHoaDon = r.HoaDonChiTiets.Sum(r => r.SoLuongBan * r.DonGiaBan),
                XemChiTiet = "Xem chi tiết"

            }).ToList();
            dataGridView.DataSource = hd;
        }

        private void btnLapHoaDon_Click(object sender, EventArgs e)
        {
            using (frmHoaDon_ChiTiet chiTiet = new frmHoaDon_ChiTiet())
            {
                chiTiet.ShowDialog();
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            id = Convert.ToInt32(dataGridView.CurrentRow.Cells["ID"].Value.ToString());
            using (frmHoaDon_ChiTiet chiTiet = new frmHoaDon_ChiTiet(id))
            {
                chiTiet.ShowDialog();
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần xóa!");
                return;
            }

            int id = Convert.ToInt32(dataGridView.CurrentRow.Cells["ID"].Value);

            if (MessageBox.Show("Bạn có muốn xóa hóa đơn này?",
                "Xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                HoaDon hd = context.HoaDons.Find(id);

                if (hd != null)
                {
                    context.HoaDons.Remove(hd);
                    context.SaveChanges();
                }

                frmHoaDon_Load(sender, e);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show("Bạn có muốn thoát không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            if (txtTimKiem.Text.Trim() == "")
            {
                frmHoaDon_Load(sender, e);
                return;
            }

            string tuKhoa = txtTimKiem.Text.Trim();

            List<DanhSachHoaDon> hd = context.HoaDons
                .Where(r => r.KhachHang.HoVaTen.Contains(tuKhoa)
                         || r.NhanVien.HoVaTen.Contains(tuKhoa))
                .Select(r => new DanhSachHoaDon
                {
                    Id = r.Id,
                    NhanVienId = r.NhanVienId,
                    HoVaTenNhanVien = r.NhanVien.HoVaTen,
                    KhachHangId = r.KhachHangId,
                    HoVaTenKhachHang = r.KhachHang.HoVaTen,
                    NgayLap = r.NgayLap,
                    GhiChuHoaDon = r.GhiChuHoaDon,
                    TongTienHoaDon = r.HoaDonChiTiets.Sum(x => x.SoLuongBan * x.DonGiaBan),
                    XemChiTiet = "Xem chi tiết"
                }).ToList();

            dataGridView.DataSource = hd;
        }

        private void btnInHoaDon_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn cần in!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dataGridView.CurrentRow.Cells["ID"].Value);

            using (frmHoaDon_ChiTiet f = new frmHoaDon_ChiTiet(id))
            {
                f.ShowDialog();
            }
        }

        private void btnXuat_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Xuất danh sách Hóa Đơn";
            saveFileDialog.Filter = "Excel|*.xls;*.xlsx";
            saveFileDialog.FileName = "HoaDon_" + DateTime.Now.ToShortDateString().Replace("/", "_") + ".xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DataTable table = new DataTable();

                    table.Columns.Add("ID", typeof(int));
                    table.Columns.Add("NhanVienID", typeof(int));
                    table.Columns.Add("KhachHangID", typeof(int));
                    table.Columns.Add("NgayLap", typeof(DateTime));
                    table.Columns.Add("GhiChuHoaDon", typeof(string));

                    var ds = context.HoaDons.ToList();

                    foreach (var p in ds)
                    {
                        table.Rows.Add(
                            p.Id,
                            p.NhanVienId,
                            p.KhachHangId,
                            p.NgayLap,
                            p.GhiChuHoaDon
                        );
                    }

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var sheet = wb.Worksheets.Add(table, "HoaDon");
                        sheet.Columns().AdjustToContents();
                        wb.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Đã xuất Excel thành công!",
                        "Thành công",
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
            openFileDialog.Title = "Nhập dữ liệu Hóa Đơn từ Excel";
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

                        if (table.Rows.Count > 0)
                        {
                            foreach (DataRow r in table.Rows)
                            {
                                HoaDon hd = new HoaDon();

                                hd.NhanVienId = int.Parse(r["NhanVienID"].ToString());
                                hd.KhachHangId = int.Parse(r["KhachHangID"].ToString());
                                hd.NgayLap = DateTime.Parse(r["NgayLap"].ToString());
                                hd.GhiChuHoaDon = r["GhiChuHoaDon"].ToString();

                                context.HoaDons.Add(hd);
                            }

                            context.SaveChanges();

                            MessageBox.Show("Đã nhập " + table.Rows.Count + " hóa đơn.",
                                "Thành công",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            frmHoaDon_Load(sender, e);
                        }
                        else
                        {
                            MessageBox.Show("File Excel rỗng!");
                        }
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
