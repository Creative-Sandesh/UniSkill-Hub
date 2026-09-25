using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace UniSkillHub
{
    /// <summary>
    /// Shared rules for uploading and downloading files.
    /// Everything is stored under /Uploads and only the RELATIVE path
    /// (for example "Uploads/Assignments/a1_s5_..._report.pdf") goes into the database.
    /// </summary>
    public static class FileHelper
    {
        // Allowed assignment submission types and the size limit (10 MB).
        public static readonly string[] SubmissionExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".zip", ".txt" };
        public const int MaxSubmissionBytes = 10 * 1024 * 1024;

        // ---------- upload checks ----------

        public static bool IsAllowedExtension(string fileName, string[] allowed)
        {
            string extension = Path.GetExtension(fileName ?? "").ToLowerInvariant();
            return Array.IndexOf(allowed, extension) >= 0;
        }

        /// <summary>
        /// Checks the first bytes of the file so that, for example, a program renamed
        /// to "report.pdf" is rejected. (The extension alone can be faked.)
        /// </summary>
        public static bool ContentMatchesExtension(HttpPostedFile file)
        {
            byte[] head = new byte[512];
            int read;

            file.InputStream.Position = 0;
            read = file.InputStream.Read(head, 0, head.Length);
            file.InputStream.Position = 0;

            if (read == 0) return false;   // empty file

            switch (Path.GetExtension(file.FileName).ToLowerInvariant())
            {
                case ".pdf":
                    return StartsWith(head, read, new byte[] { 0x25, 0x50, 0x44, 0x46 });                 // %PDF
                case ".zip":
                case ".docx":
                case ".pptx":
                case ".xlsx":
                    return StartsWith(head, read, new byte[] { 0x50, 0x4B, 0x03, 0x04 });                 // PK..
                case ".doc":
                case ".ppt":
                case ".xls":
                    return StartsWith(head, read, new byte[] { 0xD0, 0xCF, 0x11, 0xE0 });                 // old Office
                case ".mp4":                                                                                  // ....ftyp
                    return read >= 12 && head[4] == 0x66 && head[5] == 0x74 && head[6] == 0x79 && head[7] == 0x70;
                case ".webm":
                    return StartsWith(head, read, new byte[] { 0x1A, 0x45, 0xDF, 0xA3 });                 // Matroska/WebM header
                case ".ogv":
                case ".ogg":
                    return StartsWith(head, read, new byte[] { 0x4F, 0x67, 0x67, 0x53 });                 // OggS
                case ".mp3":
                    return StartsWith(head, read, new byte[] { 0x49, 0x44, 0x33 }) ||                     // "ID3" tag
                           (read >= 2 && head[0] == 0xFF && (head[1] & 0xE0) == 0xE0);                    // MPEG frame sync
                case ".wav":                                                                              // RIFF....WAVE
                    return read >= 12 && StartsWith(head, read, new byte[] { 0x52, 0x49, 0x46, 0x46 }) &&
                           head[8] == 0x57 && head[9] == 0x41 && head[10] == 0x56 && head[11] == 0x45;
                case ".txt":
                    for (int i = 0; i < read; i++)
                    {
                        if (head[i] == 0) return false;   // plain text never contains NUL bytes
                    }
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Turns a user-supplied file name into something safe to store on disk:
        /// only letters, digits, dash and underscore, and a limited length.
        /// </summary>
        public static string MakeSafeFileName(string nameWithoutExtension, int maxLength)
        {
            StringBuilder safe = new StringBuilder();
            foreach (char c in nameWithoutExtension ?? "")
            {
                if (char.IsLetterOrDigit(c) && c < 128) safe.Append(c);
                else if (c == '-' || c == '_') safe.Append(c);
                else if (c == ' ' || c == '.') safe.Append('_');
            }
            if (safe.Length == 0) safe.Append("file");
            if (safe.Length > maxLength) safe.Length = maxLength;
            return safe.ToString();
        }

        // ---------- paths ----------

        /// <summary>
        /// Full disk path of a relative path, or null if it points outside /Uploads.
        /// </summary>
        public static string ResolveInsideUploads(HttpContext context, string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;

            string root = Path.GetFullPath(context.Server.MapPath("~/Uploads")) + Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(context.Server.MapPath("~/" + relativePath));

            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? full : null;
        }

        /// <summary>
        /// Deletes an uploaded file (ignored if it is missing or outside /Uploads).
        /// </summary>
        public static void DeleteUploadedFile(HttpContext context, string relativePath)
        {
            try
            {
                string full = ResolveInsideUploads(context, relativePath);
                if (full != null && File.Exists(full)) File.Delete(full);
            }
            catch (IOException)
            {
                // A file that cannot be deleted right now is not worth failing the request.
            }
        }

        // ---------- download ----------

        /// <summary>
        /// Sends the file as a download. Returns false (and sends nothing) if the path
        /// is outside /Uploads or the file does not exist - the caller then answers 404.
        /// </summary>
        public static bool SendDownload(HttpContext context, string relativePath)
        {
            string fullPath = ResolveInsideUploads(context, relativePath);
            if (fullPath == null || !File.Exists(fullPath)) return false;

            ContentDisposition disposition = new ContentDisposition();
            disposition.FileName = Path.GetFileName(fullPath);

            context.Response.Clear();
            context.Response.ContentType = GetContentType(fullPath);
            context.Response.AddHeader("Content-Disposition", disposition.ToString());
            context.Response.AddHeader("X-Content-Type-Options", "nosniff");
            context.Response.TransmitFile(fullPath);
            return true;
        }

        public static void SendNotFound(HttpContext context)
        {
            context.Response.Clear();
            context.Response.StatusCode = 404;
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.ContentType = "text/plain";
            context.Response.Write("File not found.");
        }

        public static string GetContentType(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".pdf": return "application/pdf";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".ppt": return "application/vnd.ms-powerpoint";
                case ".pptx": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".xls": return "application/vnd.ms-excel";
                case ".xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".txt": return "text/plain";
                case ".zip": return "application/zip";
                case ".mp4": return "video/mp4";
                case ".webm": return "video/webm";
                case ".ogv": return "video/ogg";
                case ".mp3": return "audio/mpeg";
                case ".ogg": return "audio/ogg";
                case ".wav": return "audio/wav";
                default: return "application/octet-stream";
            }
        }

        private static bool StartsWith(byte[] data, int length, byte[] signature)
        {
            if (length < signature.Length) return false;
            for (int i = 0; i < signature.Length; i++)
            {
                if (data[i] != signature[i]) return false;
            }
            return true;
        }
    }
}
