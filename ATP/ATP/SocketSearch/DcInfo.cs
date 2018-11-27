using System;
using System.Collections.Generic;
using System.Text;



namespace SocketSearch
{
    class DcInfo
    {
        public ModelType DCCtrlMode = 0; //司控器的模式
        public string baliseHead = "";
        public string curBalise = "";
        public string trainHead = "";
        public string trainTail = "";    //都等于目前应答器
        public Int16 DCTrainSpeed = 0;  //实时列车速度，司控器发送，区分大于0和小于0
        public HandlePos DCHandlePos = HandlePos.HandleNone; //司控器实时传送的把柄方向，默认是0, 速度大于0是1，速度小于0是2

        internal void SetCurBalise()
        {
            curBalise = baliseHead;
            trainHead = baliseHead;
            trainTail = baliseHead;  //目前都等于当前应答器传来的消息
        }

        internal bool IsCurBaliseEmpty()
        {
            return string.IsNullOrEmpty(curBalise);
        }

        internal bool IsCurStartWith(string startWord)
        {
            return curBalise.StartsWith(startWord);
        }

        internal bool isTailStartWith(string startWord)
        {
            return trainTail.StartsWith(startWord);
        }

        internal bool isHeadTartWith(string startWord)
        {
            return trainHead.StartsWith(startWord);
        }

        internal bool IsMovingLeft()
        {
            return DCTrainSpeed < 0 && DCHandlePos == HandlePos.Handle_2;
        }

        internal bool IsMovingRight()
        {
            return DCTrainSpeed > 0 && DCHandlePos == HandlePos.Handle_1;
        }
    }
}
