## ��ȫͨ��
### SA ��ȫ����
��ȫ������ָ��������ȫͨ��Э��(IPsec)�н������ռ����йذ�ȫ����ϸ�ڵ�ժҪ��
* a one-way relationship between sender & receiver that affords security for traffic flow
* has a number of other parameters
* seq no, AH & EH info, lifetime etc
* have a database of Security Associations
#### defined by 3 parameters:
* Security Parameters Index (SPI)
* IP Destination Address
* Security Protocol Identifier
#### SA�Ĺ���
##### ����
* ��Э��SA�������ٸ���SDAB�� 
* ��ͨ���˹�������Ҳ�ɲ��ö�̬������ʽ�� 
##### ɾ�� 
* ��Ч�ڹ��ڣ�����ʱ���ʹ��SA���ֽ����ѳ��������趨��ֵ��
* ��Կ�����ƻ���
* ��һ��Ҫ��ɾ�����SA�� 
### AH ��֤ͷ
IPsec ��֤ͷЭ�飨IPsec AH���� IPsec ��ϵ�ṹ�е�һ����ҪЭ�飬��Ϊ IP ���ݱ��ṩ������������������Դ��֤�����ṩ�����Ա����ز������һ��������ȫ���ӣ����շ��Ϳ��ܻ�ѡ���һ�ַ���AH ������Ϊ IP ͷ���ϲ�Э�������ṩ�㹻�����֤��
* provides support for data integrity & authentication of IP packets
* end system/router can authenticate user/app
* prevents address spoofing attacks by tracking sequence numbers
* The limited ability of against replay attack
* based on use of a MAC
* parties must share a secret key
### ���ز�����
#### ���к��ֶ�
����һ���µ�SAʱ�������߻Ὣ���кż�������ʼ��Ϊ0�� 
ÿ������һSA�Ϸ���һ�����ݰ������кż�������ֵ�ͼ�1�������к��ֶ����óɼ�������ֵ�� 
���ﵽ�����ֵ232-1ʱ����Ӧ����һ���µ�SA�� 
#### һ�֡����������ڻ���
IP�������ӵġ����ɿ��� �����������ڣ�
���ڵ�����˶�Ӧ�ڴ�����ʼλ�õ����ݰ����к�N�������Ҷ˶�Ӧ�ڿ��Խ��յĺϷ������������N+W-1�� 
