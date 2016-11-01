// -----------------------------------------------------------------------
// <copyright file="FileUnitExecuteHelper.cs" company="Paragon Software Group">
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

using ChinhDo.Transactions.Interfaces;

namespace ChinhDo.Transactions.Heplers
{
    public static class ExecuteHelper
    {
        public static void ExecuteOperation(IFileManager fileManager, FileOperations operation, object[] param)
        {
            if (operation == FileOperations.AppendAllText)
            {
                string path = param[0].ToString();
                string contents = param[1].ToString();

                fileManager.AppendAllText(path, contents);
            }
            else if (operation == FileOperations.Copy)
            {
                string sourceFileName = param[0].ToString();
                string destFileName = param[1].ToString();
                bool overwrite = (bool)param[2];

                fileManager.Copy(sourceFileName, destFileName, overwrite);
            }
            else if (operation == FileOperations.CreateFile)
            {
                string pathToFile = param[0].ToString();
                string fileName = param[1].ToString();
                string fileExtention = param[2].ToString();

                fileManager.CreateFile(pathToFile, fileName, fileExtention);
            }
            else if (operation == FileOperations.Delete)
            {
                string path = param[0].ToString();

                fileManager.Delete(path);
            }
            else if (operation == FileOperations.Move)
            {
                string sourceFileName = param[0].ToString();
                string destFileName = param[1].ToString();

                fileManager.Move(sourceFileName, destFileName);
            }
            else if (operation == FileOperations.WriteAllText)
            {
                string path = param[0].ToString();
                string contents = param[1].ToString();

                fileManager.WriteAllText(path, contents);
            }
        }
    }
}
