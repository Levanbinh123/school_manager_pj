using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class FeeService: IFeeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // GET ALL FEES
        // ============================================================

        public async Task<List<FeeViewModel>> GetAllFees()
        {
            var repository =
                _unitOfWork.GenericRepository<Fee>();

            var fees = repository
                .GetAll()
                .ToList();

            return fees
                .Select(x => new FeeViewModel(x))
                .ToList();
        }


        // ============================================================
        // GET FEE BY ID
        // ============================================================

        public async Task<FeeViewModel?> GetFeeById(int id)
        {
            var repository =
                _unitOfWork.GenericRepository<Fee>();

            var fee = await repository
                .GetByIdAsync(x => x.Id == id);

            if (fee == null)
            {
                return null;
            }

            return new FeeViewModel(fee);
        }


        // ============================================================
        // CREATE FEE
        // ============================================================

        public async Task<FeeViewModel> CreateFee(
            CreateFeeViewModel vm)
        {
            // --------------------------------------------------------
            // 1. Kiểm tra Student
            // --------------------------------------------------------

            var studentRepository =
                _unitOfWork.GenericRepository<Student>();

            var student = await studentRepository
                .GetByIdAsync(x => x.Id == vm.StudentId);

            if (student == null)
            {
                throw new Exception(
                    "Không tìm thấy sinh viên.");
            }


            // --------------------------------------------------------
            // 2. Kiểm tra Subject
            // --------------------------------------------------------

            var subjectRepository =
                _unitOfWork.GenericRepository<Subject>();

            var subject = await subjectRepository
                .GetByIdAsync(x => x.Id == vm.SubjectId);

            if (subject == null)
            {
                throw new Exception(
                    "Không tìm thấy môn học.");
            }


            // --------------------------------------------------------
            // 3. Kiểm tra Session
            // --------------------------------------------------------

            var sessionRepository =
                _unitOfWork.GenericRepository<Session>();

            var session = await sessionRepository
                .GetByIdAsync(x => x.Id == vm.SessionId);

            if (session == null)
            {
                throw new Exception(
                    "Không tìm thấy session.");
            }


            // --------------------------------------------------------
            // 4. Kiểm tra Student đã đăng ký Subject chưa
            //
            // Student
            //     ↓
            //   Enroll
            //     ↓
            //    Fee
            // --------------------------------------------------------

            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var enroll = enrollRepository
                .GetAll()
                .FirstOrDefault(x =>
                    x.StudentId == vm.StudentId &&
                    x.SubjectId == vm.SubjectId &&
                    x.SessionId == vm.SessionId);

            if (enroll == null)
            {
                throw new Exception(
                    "Sinh viên chưa đăng ký môn học này.");
            }


            // --------------------------------------------------------
            // 5. Kiểm tra TuitionRate
            // --------------------------------------------------------

            var tuitionRateRepository =
           _unitOfWork.GenericRepository<TuitionRate>();

            var tuitionRate = tuitionRateRepository
                .GetAll()
                .FirstOrDefault(x =>
                    x.SessionId == vm.SessionId);

            if (tuitionRate == null)
            {
                throw new Exception(
                    "Không tìm thấy mức học phí của năm học.");
            }


            // --------------------------------------------------------
            // 6. Lấy số tín chỉ
            // --------------------------------------------------------

            int credits = subject.Credits;

            if (credits <= 0)
            {
                throw new Exception(
                    "Số tín chỉ của môn học không hợp lệ.");
            }


            // --------------------------------------------------------
            // 7. Lấy giá tiền / tín chỉ
            // --------------------------------------------------------

            decimal pricePerCredit =
                tuitionRate.PricePerCredit;


            // --------------------------------------------------------
            // 8. Tính tổng học phí
            // --------------------------------------------------------

            decimal amount =
                credits * pricePerCredit;


            // --------------------------------------------------------
            // 9. Kiểm tra Fee đã tồn tại chưa
            //
            // Student + Subject + Session
            // chỉ được có 1 Fee
            // --------------------------------------------------------

            var feeRepository =
                _unitOfWork.GenericRepository<Fee>();

            var exists = feeRepository
                .GetAll()
                .Any(x =>
                    x.StudentId == vm.StudentId &&
                    x.SubjectId == vm.SubjectId &&
                    x.SessionId == vm.SessionId);

            if (exists)
            {
                throw new Exception(
                    "Sinh viên đã có học phí cho môn học này.");
            }


            // --------------------------------------------------------
            // 10. Tạo Fee
            // --------------------------------------------------------

            var fee = new Fee
            {
                StudentId = vm.StudentId,
                SubjectId = vm.SubjectId,
                SessionId = vm.SessionId,

                Credits = credits,

                PricePerCredit =
                    pricePerCredit,

                Amount = amount,

                DueDate = vm.DueDate,

                Status = "Unpaid"
            };


            // --------------------------------------------------------
            // 11. Add
            // --------------------------------------------------------

            feeRepository.Add(fee);


            // --------------------------------------------------------
            // 12. Save
            // --------------------------------------------------------

            _unitOfWork.Save();


            // --------------------------------------------------------
            // 13. Lấy lại Fee
            // --------------------------------------------------------

            var result = await feeRepository
                .GetByIdAsync(x => x.Id == fee.Id);

            if (result == null)
            {
                throw new Exception(
                    "Không thể tạo học phí.");
            }


            return new FeeViewModel(result);
        }


        // ============================================================
        // UPDATE FEE
        // ============================================================

        public async Task<FeeViewModel> UpdateFee(
            int id,
            UpdateFeeViewModel vm)
        {
            var repository =
                _unitOfWork.GenericRepository<Fee>();

            var fee = await repository
                .GetByIdAsync(x => x.Id == id);

            if (fee == null)
            {
                throw new Exception(
                    "Không tìm thấy học phí.");
            }


            // --------------------------------------------------------
            // Update DueDate
            // --------------------------------------------------------

            fee.DueDate = vm.DueDate;


            // --------------------------------------------------------
            // Update Status
            // --------------------------------------------------------

            fee.Status = vm.Status;


            // --------------------------------------------------------
            // Update
            // --------------------------------------------------------

            repository.Update(fee);


            // --------------------------------------------------------
            // Save
            // --------------------------------------------------------

            _unitOfWork.Save();


            return new FeeViewModel(fee);
        }


        // ============================================================
        // DELETE FEE
        // ============================================================

        public async Task<bool> DeleteFee(int id)
        {
            var repository =
                _unitOfWork.GenericRepository<Fee>();

            var fee = await repository
                .GetByIdAsync(x => x.Id == id);

            if (fee == null)
            {
                throw new Exception(
                    "Không tìm thấy học phí.");
            }


            repository.Delete(fee);

            _unitOfWork.Save();

            return true;
        }


        // ============================================================
        // STUDENT GET MY FEES
        // ============================================================

        public async Task<List<FeeViewModel>> GetMyFees(
            string userId)
        {
            // --------------------------------------------------------
            // Tìm Student theo KeyId
            // --------------------------------------------------------

            var studentRepository =
                _unitOfWork.GenericRepository<Student>();

            var student = studentRepository
                .GetAll()
                .FirstOrDefault(x =>
                    x.KeyId == userId);

            if (student == null)
            {
                throw new Exception(
                    "Không tìm thấy sinh viên.");
            }


            // --------------------------------------------------------
            // Lấy Fee của Student
            // --------------------------------------------------------

            var feeRepository =
                _unitOfWork.GenericRepository<Fee>();

            var fees = feeRepository
                .GetAll()
                .Where(x =>
                    x.StudentId == student.Id)
                .ToList();


            return fees
                .Select(x => new FeeViewModel(x))
                .ToList();
        }


        // ============================================================
        // STUDENT GET MY FEE BY ID
        // ============================================================

        public async Task<FeeViewModel?> GetMyFee(
            string userId,
            int feeId)
        {
            // --------------------------------------------------------
            // Tìm Student
            // --------------------------------------------------------

            var studentRepository =
                _unitOfWork.GenericRepository<Student>();

            var student = studentRepository
                .GetAll()
                .FirstOrDefault(x =>
                    x.KeyId == userId);

            if (student == null)
            {
                throw new Exception(
                    "Không tìm thấy sinh viên.");
            }


            // --------------------------------------------------------
            // Tìm Fee
            // --------------------------------------------------------

            var feeRepository =
                _unitOfWork.GenericRepository<Fee>();

            var fee = await feeRepository
                .GetByIdAsync(x =>
                    x.Id == feeId &&
                    x.StudentId == student.Id);

            if (fee == null)
            {
                return null;
            }


            return new FeeViewModel(fee);
        }
    }
}