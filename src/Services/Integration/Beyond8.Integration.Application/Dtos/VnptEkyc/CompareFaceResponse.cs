using System;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;

public class CompareFaceResponse
{
    public string Result { get; set; } = string.Empty;
    public string Msg { get; set; } = string.Empty;
    public double Prob { get; set; }
}
