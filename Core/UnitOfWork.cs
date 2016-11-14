// -----------------------------------------------------------------------
// <copyright file="UnitOfWork.cs" company="Paragon Software Group">
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

namespace Core
{
    using System.IO;
    using Core.Helpers;
    using System.Linq;

    public sealed class UnitOfWork
    {
        public UnitOfWork() : this(true)
        {
        }
        
        public UnitOfWork(bool checkAfterCrush)
        {
            FolderHelper.CreateJournalsFolder();
            if (checkAfterCrush)
            {
                this.CheckBadTransaction();
            }
        }

        public BussinesTransaction BeginTransaction()
        {
            return new BussinesTransaction();
        }

        private void CheckBadTransaction()
        {
            var journals = Directory.GetFiles(FolderHelper.JournalsFolder);
            if (journals.Any())
            {
                this.RollbackBadTransactions(journals);
            }
        }

        private void RollbackBadTransactions(string[] journals)
        {
            foreach (var journalPath in journals)
            {
                using (new BadBussinesTransaction(journalPath))
                {
                }
            }
        }
    }
}
