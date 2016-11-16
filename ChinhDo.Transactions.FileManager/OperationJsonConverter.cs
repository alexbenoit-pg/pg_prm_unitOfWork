// -----------------------------------------------------------------------
// <copyright file="OperationJsonConverter.cs" company="Paragon Software Group">
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

namespace FileTransactionManager
{
    using System;
    using FileTransactionManager.Interfaces;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using FileTransactionManager.Operations;

    public class OperationJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IRollbackableOperation);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var unit = default(IRollbackableOperation);
            switch (jsonObject["Type"].Value<string>())
            {
                case "AppendAllText":
                    unit = new AppendAllTextOperation(
                        jsonObject["path"].Value<string>(), 
                        jsonObject["contents"].Value<string>());
                    break;
                case "Copy":
                    unit = new CopyOperation(
                        jsonObject["sourceFileName"].Value<string>(),
                        jsonObject["destFileName"].Value<string>(),
                        jsonObject["overwrite"].Value<bool>());
                    break;
                case "CreateFile":
                    unit = new CreateFileOperation(
                        jsonObject["pathToFile"].Value<string>());
                    break;
                case "Delete":
                    unit = new DeleteFileOperation(
                        jsonObject["path"].Value<string>());
                    break;
                case "Move":
                    unit = new MoveOperation(
                        jsonObject["sourceFileName"].Value<string>(),
                        jsonObject["destFileName"].Value<string>());
                    break;
                case "WriteAllText":
                    unit = new WriteAllTextOperation(
                        jsonObject["path"].Value<string>(),
                        jsonObject["contents"].Value<string>());
                    break;
            }
            serializer.Populate(jsonObject.CreateReader(), unit);
            return unit;
        }
    }
}
