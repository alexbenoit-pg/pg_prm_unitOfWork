// -----------------------------------------------------------------------
// <copyright file="BussinesTransaction.cs" company="Paragon Software Group">
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
    using System;
    using System.Collections.Generic;
    using Core.Interfaces;
    
    public sealed class BussinesTransaction : IDisposable
    {
        private List<ITransactionUnit> executedUnits;
        private ISaver saver;
        private bool isException;
        
        internal BussinesTransaction(ISaver saver)
        {
            this.executedUnits = new List<ITransactionUnit>();
            this.saver = saver;
            this.isException = true;
        }

        public void ExecuteUnit(ITransactionUnit unit)
        {
            try
            {
                unit.Commit();
                this.executedUnits.Add(unit);
                this.saver.Save(this.executedUnits);
            }
            catch (Exception)
            {
                this.Dispose();
            }
        }

        public void Commit()
        {
            this.isException = false;
        }

        public void Dispose()
        {
            if (this.isException)
            {
                this.Rollback();
            }

            this.saver.Dispose();
            this.executedUnits.Clear();
            this.executedUnits = null;
        }
        
        private void Rollback()
        {
            var notRollbacked = new List<ITransactionUnit>();
            notRollbacked.AddRange(this.executedUnits);

            foreach (var unit in this.executedUnits)
            {
                unit.Rollback();
                notRollbacked.Remove(unit);
                this.saver.Save(notRollbacked);
            }
        }
    }
}