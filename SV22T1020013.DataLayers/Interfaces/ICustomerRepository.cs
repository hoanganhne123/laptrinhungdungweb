using SV22T1020013.Models.Partner;

namespace SV22T1020013.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Customer
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Kiểm tra xem một địa chỉ email có hợp lệ hay không?
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của khách hàng mới.
        /// Nếu id <> 0: Kiểm tra email đối với khách hàng đã tồn tại
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);

        /// <summary>
        /// Đổi mật khẩu của khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <param name="newPassword">Mật khẩu mới đã được mã hóa MD5</param>
        /// <returns>True nếu đổi thành công, ngược lại False</returns>
        Task<bool> ChangePasswordAsync(int id, string newPassword);
    }
}
