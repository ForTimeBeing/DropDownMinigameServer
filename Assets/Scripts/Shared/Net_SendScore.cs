[System.Serializable]
public class Net_SendScore : NetMsg
{
    public Net_SendScore()
    {
        OP = NetOP.SendScore;
    }

    public int score { set; get; }
}
