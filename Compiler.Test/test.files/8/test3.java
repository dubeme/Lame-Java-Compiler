class classone{
  public void firstclass(){
    return;
  }
  public int secondclass(int a,int b, int d){
    int c;

    c=a*b+d;
    write("The answer is ");
    write(c);
    writeln(" ");
    return c;
  }
  public int thirdclass(){
    int a,b,d;
    a=5;
    b=10;
    d=20;
    classone.secondclass(b,d,a);
    return 2;
  }
}
final class Main{
  public static void main(String [] args){
    classone.thirdclass();
  }
}
