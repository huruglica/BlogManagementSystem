using Microsoft.AspNetCore.Http;

namespace BlogManagementSystemTest.BlogHelpers
{
    public class FakeFormFile : IFormFile
    {
        public string ContentDisposition { get; set; }
        public string ContentType { get; set; }
        public IHeaderDictionary Headers { get; set; }
        public long Length { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public Stream OpenReadStream() => new MemoryStream();

        public void CopyTo(Stream target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            using (Stream sourceStream = OpenReadStream())
            {
                sourceStream.CopyTo(target);
            }
        }

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            using (Stream sourceStream = OpenReadStream())
            {
                 await sourceStream.CopyToAsync(target);
            }
        }
    }
}
