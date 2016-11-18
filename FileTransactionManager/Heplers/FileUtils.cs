// -----------------------------------------------------------------------
// <copyright file="FileUtils.cs" company="Paragon Software Group">
// EXCEPT WHERE OTHERWISE STATED, THE INFORMATION AND SOURCE CODE CONTAINED 
// HEREIN AND IN RELATED FILES IS THE EXCLUSIVE PROPERTY OF PARAGON SOFTWARE
// GROUP COMPANY AND MAY NOT BE EXAMINED, DISTRIBUTED, DISCLOSED, OR REPRODUCED
// IN WHOLE OR IN PART WITHOUT EXPLICIT WRITTEN AUTHORIZATION FROM THE COMPANY.
// 
// Copyright (c) 1994-2016 Paragon Software Group, All rights reserved.
// 
// UNLESS OTHERWISE AGREED IN A WRITING SIGNED BY THE PARTIES, THIS SOFTWARE IS
// PROVIDED "AS-IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
// PARTICULAR PURPOSE, ALL OF WHICH ARE HEREBY DISCLAIMED. IN NO EVENT SHALL THE
// AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF NOT ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// -----------------------------------------------------------------------

namespace FileTransactionManager.Heplers
{
    using System;
    using System.IO;

    public static class FileUtils
    {
        private static readonly string TempFolder = Path.Combine(Path.GetTempPath(), "UnitOfWorkFileTransaction");
        private static readonly string JournalFolder = Path.Combine(TempFolder, "Journals");

        /// <summary>
        /// Ensures that the folder that contains the temporary files exists.
        /// </summary>
        public static void EnsureTempFolderExists()
        {
            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
        }

        /// <summary>
        /// Ensures that the folder that contains the temporary files exists.
        /// </summary>
        public static void EnsureJournalFolder()
        {
            if (!Directory.Exists(JournalFolder))
            {
                Directory.CreateDirectory(JournalFolder);
            }
        }

        /// <summary>
        /// Returns a unique temporary file name.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetTempFileName(string extension)
        {
            Guid g = Guid.NewGuid();
            string retVal = Path.Combine(TempFolder, g.ToString().Substring(0, 8)) + extension;

            return retVal;
        }
    }
}
